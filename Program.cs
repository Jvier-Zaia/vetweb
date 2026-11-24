using Microsoft.EntityFrameworkCore;
using VetWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Cargar configuración local opcional para API Keys privadas (ignorado en Git)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Configuración de MariaDB / MySQL con Pomelo EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Usamos versión fija para evitar excepciones al vuelo en AutoDetect si MySQL tarda en responder
builder.Services.AddDbContext<VetDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 3))));

var app = builder.Build();

// Crear tabla pedidos si no existe en MariaDB
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<VetDbContext>();
        // Si la base está vacía (p. ej. un MySQL nuevo en Railway), cargar esquema y datos de muestra
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using (var check = conn.CreateCommand())
        {
            check.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'productos'";
            var existe = Convert.ToInt32(check.ExecuteScalar()) > 0;
            var seedPath = Path.Combine(AppContext.BaseDirectory, "veterinaria_db.sql");
            if (!existe && File.Exists(seedPath))
            {
                var lines = File.ReadAllLines(seedPath).Where(l =>
                    !l.TrimStart().StartsWith("DROP DATABASE", StringComparison.OrdinalIgnoreCase) &&
                    !l.TrimStart().StartsWith("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) &&
                    !l.TrimStart().StartsWith("USE ", StringComparison.OrdinalIgnoreCase));
                using var seed = conn.CreateCommand();
                seed.CommandText = string.Join("\n", lines);
                seed.ExecuteNonQuery();
                Console.WriteLine("[SEED] Esquema y datos de muestra cargados.");
            }
        }
        conn.Close();

        string sqlPedidos = @"
            CREATE TABLE IF NOT EXISTS pedidos (
                id_pedido INT AUTO_INCREMENT PRIMARY KEY,
                cliente_nombre VARCHAR(80) NOT NULL,
                cliente_telefono VARCHAR(25) NOT NULL,
                cliente_email VARCHAR(100) NOT NULL,
                total DECIMAL(10,2) NOT NULL,
                alias_destino VARCHAR(50) DEFAULT 'vetweb.oficial',
                nro_operacion VARCHAR(60),
                comprobante_nombre VARCHAR(150),
                estado ENUM('Pendiente', 'Verificado', 'Rechazado') DEFAULT 'Verificado',
                fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                detalle_items TEXT
            ) ENGINE=InnoDB;";
        db.Database.ExecuteSqlRaw(sqlPedidos);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[AVISO DE BASE DE DATOS] No se pudo conectar a MySQL en localhost:3306 ({ex.Message}).");
        Console.WriteLine("[AVISO] Por favor inicia Laragon ('Start All') para habilitar la persistencia de datos.");
        Console.ResetColor();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// La Tienda es la página principal de la web
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tienda}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
