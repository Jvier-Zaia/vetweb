using System.ComponentModel.DataAnnotations;

namespace VetWeb.Models;

public class SolicitudTurnoViewModel
{
    // 1. Datos del Paciente
    [Required(ErrorMessage = "Por favor indica el nombre de tu mascota")]
    [Display(Name = "Nombre de la Mascota")]
    public string NombreMascota { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona la especie de tu mascota")]
    [Display(Name = "Especie")]
    public string Especie { get; set; } = "Perro";

    [Display(Name = "Raza (opcional)")]
    public string? Raza { get; set; }

    [Display(Name = "Edad aproximada en años (opcional)")]
    [Range(0, 40, ErrorMessage = "Ingresa una edad válida")]
    public int? Edad { get; set; }

    // 2. Datos del Dueño / Contacto
    [Required(ErrorMessage = "Por favor ingresa tu nombre y apellido")]
    [Display(Name = "Nombre y Apellido del Dueño")]
    public string NombreDuenio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tu teléfono es necesario para contactarte")]
    [Phone(ErrorMessage = "Ingresa un número de teléfono válido")]
    [Display(Name = "Teléfono de Contacto")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es indispensable para enviarte la confirmación")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    // 3. Datos de la Consulta
    [Required(ErrorMessage = "Selecciona la fecha y hora de la cita")]
    [Display(Name = "Fecha y Hora deseada")]
    public DateTime FechaHora { get; set; } = DateTime.Now.AddDays(1).Date.AddHours(10); // Mañana a las 10:00

    [Required(ErrorMessage = "Describe brevemente el motivo de la consulta")]
    [Display(Name = "Motivo de la Consulta")]
    public string Motivo { get; set; } = string.Empty;

    [Display(Name = "Profesional Veterinario")]
    public int IdVeterinario { get; set; }
}
