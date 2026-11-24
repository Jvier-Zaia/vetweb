using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetWeb.Models;

[Table("especies")]
public class Especie
{
    [Key]
    [Column("id_especie")]
    public int IdEspecie { get; set; }

    [Required]
    [Column("nombre")]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;
}

[Table("duenios")]
public class Duenio
{
    [Key]
    [Column("id_duenio")]
    public int IdDuenio { get; set; }

    [Required]
    [Column("nombre")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("apellido")]
    [StringLength(60)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [Column("telefono")]
    [StringLength(25)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellido}";
}

[Table("mascotas")]
public class Mascota
{
    [Key]
    [Column("id_mascota")]
    public int IdMascota { get; set; }

    [Required]
    [Column("nombre")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Column("id_especie")]
    public int IdEspecie { get; set; }

    [ForeignKey(nameof(IdEspecie))]
    public Especie? Especie { get; set; }

    [Column("id_duenio")]
    public int IdDuenio { get; set; }

    [ForeignKey(nameof(IdDuenio))]
    public Duenio? Duenio { get; set; }

    [Column("raza")]
    [StringLength(60)]
    public string? Raza { get; set; }

    [Column("edad")]
    public int Edad { get; set; }

    [Column("peso", TypeName = "decimal(5,2)")]
    public decimal? Peso { get; set; }
}

[Table("categorias_productos")]
public class CategoriaProducto
{
    [Key]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Required]
    [Column("nombre")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    [StringLength(150)]
    public string? Descripcion { get; set; }
}

[Table("proveedores")]
public class Proveedor
{
    [Key]
    [Column("id_proveedor")]
    public int IdProveedor { get; set; }

    [Required]
    [Column("nombre")]
    [StringLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Column("contacto")]
    [StringLength(60)]
    public string? Contacto { get; set; }

    [Column("telefono")]
    [StringLength(25)]
    public string? Telefono { get; set; }
}

[Table("productos")]
public class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [ForeignKey(nameof(IdCategoria))]
    public CategoriaProducto? Categoria { get; set; }

    [Column("id_proveedor")]
    public int IdProveedor { get; set; }

    [ForeignKey(nameof(IdProveedor))]
    public Proveedor? Proveedor { get; set; }

    [Required]
    [Column("precio", TypeName = "decimal(10,2)")]
    [Range(0.01, 1000000.0, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    [Required]
    [Column("stock")]
    [Range(0, 10000, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }

    [Column("descripcion")]
    public string? Descripcion { get; set; }
}

[Table("veterinarios")]
public class Veterinario
{
    [Key]
    [Column("id_veterinario")]
    public int IdVeterinario { get; set; }

    [Required]
    [Column("nombre")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("apellido")]
    [StringLength(60)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [Column("matricula")]
    [StringLength(30)]
    public string Matricula { get; set; } = string.Empty;

    [Column("especialidad")]
    [StringLength(60)]
    public string Especialidad { get; set; } = "Clínica General";

    [NotMapped]
    public string NombreCompleto => $"{Nombre} {Apellido} ({Especialidad})";
}

[Table("turnos")]
public class Turno
{
    [Key]
    [Column("id_turno")]
    public int IdTurno { get; set; }

    [Column("id_mascota")]
    public int IdMascota { get; set; }

    [ForeignKey(nameof(IdMascota))]
    public Mascota? Mascota { get; set; }

    [Column("id_veterinario")]
    public int IdVeterinario { get; set; }

    [ForeignKey(nameof(IdVeterinario))]
    public Veterinario? Veterinario { get; set; }

    [Required]
    [Column("fecha_hora")]
    [Display(Name = "Fecha y Hora")]
    public DateTime FechaHora { get; set; }

    [Required(ErrorMessage = "Ingresa el motivo del turno")]
    [Column("motivo")]
    [StringLength(200)]
    public string Motivo { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En Consulta, Finalizado, Cancelado
}

[Table("pedidos")]
public class Pedido
{
    [Key]
    [Column("id_pedido")]
    public int IdPedido { get; set; }

    [Required]
    [Column("cliente_nombre")]
    [StringLength(80)]
    public string ClienteNombre { get; set; } = string.Empty;

    [Required]
    [Column("cliente_telefono")]
    [StringLength(25)]
    public string ClienteTelefono { get; set; } = string.Empty;

    [Required]
    [Column("cliente_email")]
    [EmailAddress]
    [StringLength(100)]
    public string ClienteEmail { get; set; } = string.Empty;

    [Column("total", TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    [Column("alias_destino")]
    [StringLength(50)]
    public string AliasDestino { get; set; } = "vetweb.oficial";

    [Column("nro_operacion")]
    [StringLength(60)]
    public string? NroOperacion { get; set; }

    [Column("comprobante_nombre")]
    [StringLength(150)]
    public string? ComprobanteNombre { get; set; }

    [Column("estado")]
    public string Estado { get; set; } = "Verificado"; // Pendiente, Verificado, Rechazado

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.Now;

    [Column("detalle_items")]
    public string DetalleItems { get; set; } = string.Empty;
}
