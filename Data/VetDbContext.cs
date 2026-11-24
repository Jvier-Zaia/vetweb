using Microsoft.EntityFrameworkCore;
using VetWeb.Models;

namespace VetWeb.Data;

public class VetDbContext : DbContext
{
    public VetDbContext(DbContextOptions<VetDbContext> options) : base(options)
    {
    }

    public DbSet<Especie> Especies => Set<Especie>();
    public DbSet<Duenio> Duenios => Set<Duenio>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    public DbSet<CategoriaProducto> CategoriasProductos => Set<CategoriaProducto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Turno> Turnos => Set<Turno>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
}
