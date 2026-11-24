using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VetWeb.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Por favor ingresa tu nombre completo")]
    [Display(Name = "Nombre y Apellido")]
    public string ClienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Por favor ingresa un teléfono")]
    [Display(Name = "Teléfono")]
    public string ClienteTelefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Por favor ingresa tu correo electrónico")]
    [EmailAddress(ErrorMessage = "Ingresa un email válido")]
    [Display(Name = "Correo Electrónico")]
    public string ClienteEmail { get; set; } = string.Empty;

    [Display(Name = "Número de Operación")]
    public string? NroOperacion { get; set; }

    [Display(Name = "Comprobante de Transferencia")]
    public IFormFile? Comprobante { get; set; }

    [Required]
    public string CarritoJson { get; set; } = "[]";
}

public class ItemCarritoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Cantidad { get; set; }
}
