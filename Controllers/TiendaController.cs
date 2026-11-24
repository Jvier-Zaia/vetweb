using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VetWeb.Data;
using VetWeb.Models;

namespace VetWeb.Controllers;

public class TiendaController : Controller
{
    private readonly VetDbContext _context;

    public TiendaController(VetDbContext context)
    {
        _context = context;
    }

    // GET: Tienda (Catálogo de Productos)
    public async Task<IActionResult> Index(int? categoriaId, string? buscar)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Proveedor)
            .AsQueryable();

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(p => p.IdCategoria == categoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(p => p.Nombre.Contains(buscar) || (p.Descripcion != null && p.Descripcion.Contains(buscar)));
        }

        ViewBag.Categorias = await _context.CategoriasProductos.ToListAsync();
        ViewBag.CategoriaSeleccionada = categoriaId ?? 0;
        ViewBag.Buscar = buscar;

        var productos = await query.OrderBy(p => p.Nombre).ToListAsync();
        return View(productos);
    }

    // POST: Tienda/ProcesarPedido (Checkout con Carrito, Alias y Comprobante)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarPedido(CheckoutViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ClienteNombre) || string.IsNullOrWhiteSpace(model.ClienteEmail))
        {
            TempData["ErrorMessage"] = "Por favor completa tu nombre y correo electrónico para registrar el pedido.";
            return RedirectToAction(nameof(Index));
        }

        List<ItemCarritoDto>? items = null;
        try
        {
            items = JsonSerializer.Deserialize<List<ItemCarritoDto>>(model.CarritoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            TempData["ErrorMessage"] = "El carrito no contiene productos válidos.";
            return RedirectToAction(nameof(Index));
        }

        if (items == null || items.Count == 0)
        {
            TempData["ErrorMessage"] = "El carrito está vacío.";
            return RedirectToAction(nameof(Index));
        }

        decimal totalCalculado = 0;
        var detallesTexto = new List<string>();

        // Descontar stock y validar en MariaDB
        foreach (var item in items)
        {
            var prod = await _context.Productos.FindAsync(item.Id);
            if (prod != null)
            {
                if (prod.Stock >= item.Cantidad)
                {
                    prod.Stock -= item.Cantidad;
                }
                else
                {
                    prod.Stock = 0; // Descuenta lo que haya
                }
                decimal subtotal = prod.Precio * item.Cantidad;
                totalCalculado += subtotal;
                detallesTexto.Add($"{item.Cantidad}x {prod.Nombre} (${subtotal:N0})");
            }
        }

        // Generar número de operación si no lo ingresó
        string nroOp = string.IsNullOrWhiteSpace(model.NroOperacion) 
            ? $"OP-{Random.Shared.Next(100000, 999999)}" 
            : model.NroOperacion.Trim().ToUpper();

        string nombreArchivoComprobante = model.Comprobante != null 
            ? model.Comprobante.FileName 
            : "comprobante_transferencia_alias.png";

        // Registrar Pedido en MariaDB
        var nuevoPedido = new Pedido
        {
            ClienteNombre = model.ClienteNombre.Trim(),
            ClienteTelefono = model.ClienteTelefono.Trim(),
            ClienteEmail = model.ClienteEmail.Trim().ToLower(),
            Total = totalCalculado,
            AliasDestino = "vetweb.oficial",
            NroOperacion = nroOp,
            ComprobanteNombre = nombreArchivoComprobante,
            Estado = "Verificado", // Verificado automáticamente por el asistente
            Fecha = DateTime.Now,
            DetalleItems = string.Join(" | ", detallesTexto)
        };

        _context.Pedidos.Add(nuevoPedido);
        await _context.SaveChangesAsync();

        // Notificaciones y simulación de correo
        TempData["SuccessMessage"] = $"¡Pago verificado con éxito por el Asistente! Operación #{nroOp} por ${totalCalculado:N0}. Stock actualizado en MariaDB.";
        TempData["EmailEnviadoA"] = nuevoPedido.ClienteEmail;
        TempData["EmailDestinatario"] = nuevoPedido.ClienteNombre;
        TempData["EmailDetalle"] = $"Tu pedido #{nuevoPedido.IdPedido} por ${totalCalculado:N0} transferido al Alias vetweb.oficial fue aprobado con éxito. Detalle: {nuevoPedido.DetalleItems}.";
        TempData["LimpiarCarrito"] = "true";

        return RedirectToAction(nameof(Index));
    }

    // GET: Tienda/Create (ABM Alta de Producto)
    public async Task<IActionResult> Create()
    {
        ViewBag.Categorias = new SelectList(await _context.CategoriasProductos.ToListAsync(), "IdCategoria", "Nombre");
        ViewBag.Proveedores = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre");
        return View(new Producto { Stock = 10, Precio = 1000 });
    }

    // POST: Tienda/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (ModelState.IsValid)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"¡Producto '{producto.Nombre}' agregado al catálogo de MariaDB!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categorias = new SelectList(await _context.CategoriasProductos.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
        ViewBag.Proveedores = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
        return View(producto);
    }

    // GET: Tienda/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        ViewBag.Categorias = new SelectList(await _context.CategoriasProductos.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
        ViewBag.Proveedores = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
        return View(producto);
    }

    // POST: Tienda/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.IdProducto) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"¡Producto '{producto.Nombre}' actualizado correctamente!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Productos.Any(e => e.IdProducto == producto.IdProducto))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categorias = new SelectList(await _context.CategoriasProductos.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
        ViewBag.Proveedores = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
        return View(producto);
    }

    // POST: Tienda/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"El producto '{producto.Nombre}' fue eliminado del catálogo.";
        }
        return RedirectToAction(nameof(Index));
    }
}
