using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetWeb.Data;
using VetWeb.Models;

namespace VetWeb.Controllers;

public class MascotasController : Controller
{
    private readonly VetDbContext _context;

    public MascotasController(VetDbContext context)
    {
        _context = context;
    }

    // GET: Mascotas
    public async Task<IActionResult> Index(string? buscar)
    {
        var query = _context.Mascotas
            .Include(m => m.Especie)
            .Include(m => m.Duenio)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(m => m.Nombre.Contains(buscar) 
                                  || (m.Duenio != null && m.Duenio.Nombre.Contains(buscar))
                                  || (m.Duenio != null && m.Duenio.Apellido.Contains(buscar))
                                  || (m.Raza != null && m.Raza.Contains(buscar)));
        }

        ViewBag.Buscar = buscar;
        var mascotas = await query.OrderBy(m => m.Nombre).ToListAsync();
        return View(mascotas);
    }

    // GET: Mascotas/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Especies = new SelectList(await _context.Especies.ToListAsync(), "IdEspecie", "Nombre");
        ViewBag.Duenios = new SelectList(await _context.Duenios.ToListAsync(), "IdDuenio", "NombreCompleto");
        return View(new Mascota());
    }

    // POST: Mascotas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Mascota mascota)
    {
        if (ModelState.IsValid)
        {
            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"¡Paciente {mascota.Nombre} registrado con éxito en MariaDB!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Especies = new SelectList(await _context.Especies.ToListAsync(), "IdEspecie", "Nombre", mascota.IdEspecie);
        ViewBag.Duenios = new SelectList(await _context.Duenios.ToListAsync(), "IdDuenio", "NombreCompleto", mascota.IdDuenio);
        return View(mascota);
    }
}
