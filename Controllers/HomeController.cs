using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetWeb.Data;

namespace VetWeb.Controllers;

public class HomeController : Controller
{
    private readonly VetDbContext _context;

    public HomeController(VetDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalProductos = await _context.Productos.CountAsync();
        ViewBag.TotalTurnosPendientes = await _context.Turnos.CountAsync(t => t.Estado == "Pendiente");
        ViewBag.TotalMascotas = await _context.Mascotas.CountAsync();
        ViewBag.TotalVeterinarios = await _context.Veterinarios.CountAsync();

        // Próximos turnos con JOIN
        var proximosTurnos = await _context.Turnos
            .Include(t => t.Mascota)
                .ThenInclude(m => m!.Duenio)
            .Include(t => t.Veterinario)
            .OrderBy(t => t.FechaHora)
            .Take(5)
            .ToListAsync();

        // Productos destacados de la tienda
        var productosDestacados = await _context.Productos
            .Include(p => p.Categoria)
            .Take(4)
            .ToListAsync();

        ViewBag.ProximosTurnos = proximosTurnos;
        ViewBag.ProductosDestacados = productosDestacados;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
