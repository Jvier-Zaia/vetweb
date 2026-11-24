using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetWeb.Data;
using VetWeb.Models;

namespace VetWeb.Controllers;

public class TurnosController : Controller
{
    private readonly VetDbContext _context;

    public TurnosController(VetDbContext context)
    {
        _context = context;
    }

    // GET: Turnos
    public async Task<IActionResult> Index()
    {
        await CargarVeterinariosViewBag();

        // Listar los turnos existentes para ver la agenda
        var turnos = await _context.Turnos
            .Include(t => t.Mascota)
                .ThenInclude(m => m!.Duenio)
            .Include(t => t.Mascota)
                .ThenInclude(m => m!.Especie)
            .Include(t => t.Veterinario)
            .OrderBy(t => t.FechaHora)
            .ToListAsync();

        ViewBag.ListaTurnos = turnos;

        var modelo = new SolicitudTurnoViewModel
        {
            FechaHora = DateTime.Now.AddDays(1).Date.AddHours(10)
        };

        return View(modelo);
    }

    // POST: Turnos (Agendar Cita con datos simplificados)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SolicitudTurnoViewModel model)
    {
        if (ModelState.IsValid)
        {
            // 1. Buscar o registrar al Dueño por email
            var emailNorm = model.Email.Trim().ToLower();
            var duenio = await _context.Duenios.FirstOrDefaultAsync(d => d.Email.ToLower() == emailNorm);

            if (duenio == null)
            {
                // Separar nombre y apellido si es posible
                var partes = model.NombreDuenio.Trim().Split(' ', 2);
                string nombre = partes[0];
                string apellido = partes.Length > 1 ? partes[1] : "Cliente";

                duenio = new Duenio
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Telefono = model.Telefono.Trim(),
                    Email = emailNorm
                };
                _context.Duenios.Add(duenio);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Actualizar teléfono si cambió
                duenio.Telefono = model.Telefono.Trim();
                await _context.SaveChangesAsync();
            }

            // 2. Buscar o registrar la Especie
            var especie = await _context.Especies.FirstOrDefaultAsync(e => e.Nombre == model.Especie);
            if (especie == null)
            {
                especie = new Especie { Nombre = model.Especie };
                _context.Especies.Add(especie);
                await _context.SaveChangesAsync();
            }

            // 3. Buscar o registrar la Mascota del dueño
            var nombreMascotaNorm = model.NombreMascota.Trim();
            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => 
                m.IdDuenio == duenio.IdDuenio && m.Nombre.ToLower() == nombreMascotaNorm.ToLower());

            if (mascota == null)
            {
                mascota = new Mascota
                {
                    Nombre = nombreMascotaNorm,
                    IdEspecie = especie.IdEspecie,
                    IdDuenio = duenio.IdDuenio,
                    Raza = string.IsNullOrWhiteSpace(model.Raza) ? "Mestizo / Sin especificar" : model.Raza.Trim(),
                    Edad = model.Edad ?? 0,
                    Peso = null // Como el usuario no sabe el peso, queda en nulo sin obligarlo
                };
                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync();
            }

            // 4. Registrar el Turno en MariaDB
            var turno = new Turno
            {
                IdMascota = mascota.IdMascota,
                IdVeterinario = model.IdVeterinario,
                FechaHora = model.FechaHora,
                Motivo = model.Motivo.Trim(),
                Estado = "Pendiente"
            };

            _context.Turnos.Add(turno);
            await _context.SaveChangesAsync();

            // Cargar datos del veterinario para el correo
            var vet = await _context.Veterinarios.FindAsync(model.IdVeterinario);

            // Generar notificación simulada de correo
            TempData["SuccessMessage"] = $"¡Turno solicitado con éxito para {mascota.Nombre}!";
            TempData["EmailEnviadoA"] = duenio.Email;
            TempData["EmailDestinatario"] = $"{duenio.Nombre} {duenio.Apellido}";
            TempData["EmailDetalle"] = $"Cita agendada para {mascota.Nombre} ({especie.Nombre}) con {vet?.NombreCompleto ?? "Veterinario"} el día {turno.FechaHora:dd/MM/yyyy HH:mm} hs. Motivo: {turno.Motivo}";

            return RedirectToAction(nameof(Index));
        }

        await CargarVeterinariosViewBag();
        ViewBag.ListaTurnos = await _context.Turnos
            .Include(t => t.Mascota).ThenInclude(m => m!.Duenio)
            .Include(t => t.Mascota).ThenInclude(m => m!.Especie)
            .Include(t => t.Veterinario)
            .OrderBy(t => t.FechaHora)
            .ToListAsync();

        return View(model);
    }

    // POST: Turnos/CambiarEstado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno != null)
        {
            turno.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"El turno #{id} ahora está en estado '{nuevoEstado}'.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarVeterinariosViewBag()
    {
        var vets = await _context.Veterinarios
            .Select(v => new {
                v.IdVeterinario,
                Texto = $"{v.Nombre} {v.Apellido} - {v.Especialidad}"
            })
            .ToListAsync();

        ViewBag.Veterinarios = new SelectList(vets, "IdVeterinario", "Texto");
    }
}
