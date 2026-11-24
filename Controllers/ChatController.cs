using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VetWeb.Models;

namespace VetWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;

        private const string SystemPrompt = @"Eres 'VetBot', el asistente virtual veterinario inteligente de 'VetWeb'.
VetWeb es una clínica veterinaria integral y Pet Shop oficial.
Servicios de VetWeb:
- Turnos médicos: consultas clínicas, vacunación, desparasitación, cirugías, control general de perros, gatos y otras especies.
- Pet Shop: alimentos balanceados premium (Royal Canin, Pro Plan, etc.), accesorios, correas, juguetes y cuidado pet (pipetas, antipulgas).
- Métodos de pago en tienda: Transferencia bancaria directa con Alias oficial 'vetweb.oficial'.
- Atención y horarios: Lunes a Sábados de 8:00 a 20:00 hs. Urgencias 24 hs.
Pautas de respuesta:
1. Responde siempre de manera cálida, empática, profesional y concisa (1 a 3 párrafos cortos).
2. Si el usuario describe síntomas graves de una mascota (vómitos continuos, convulsiones, letargo extremo, dificultad respiratoria, sangrado o intoxicación), aconséjale de inmediato acudir a urgencias o reservar un turno médico de emergencia.
3. Puedes guiar a los clientes sobre cómo usar la web (ir a 'Pacientes (ABM)' para ver o registrar mascotas, 'Turnos' para solicitar una cita, o 'Pet Shop Oficial' para comprar).
4. Utiliza emojis amigables relacionados con mascotas (🐾, 🐶, 🐱, 🩺, 💊).";

        public ChatController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            ILogger<ChatController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensaje([FromBody] ChatRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ChatResponseDto
                {
                    Success = false,
                    Reply = "Por favor ingresa un mensaje válido para consultar con VetBot."
                });
            }

            // Clave de API de Gemini: busca en Variable de Entorno primero, luego en configuración local
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                         ?? _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "TU_API_KEY_AQUI")
            {
                return Ok(new ChatResponseDto
                {
                    Success = false,
                    Reply = "¡Hola! 🐾 Soy VetBot. Mi servicio de inteligencia artificial está listo, pero falta configurar la API Key de Gemini en el servidor (en `appsettings.Local.json` o como variable de entorno `GEMINI_API_KEY`). Mientras tanto, puedes explorar la Tienda, gestionar Mascotas o solicitar un Turno médico desde el menú superior."
                });
            }

            var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            try
            {
                var contents = new List<object>();

                // Agregar historial reciente (máximo 8 mensajes para rapidez)
                if (request.History != null && request.History.Count > 0)
                {
                    foreach (var h in request.History.TakeLast(8))
                    {
                        if (string.IsNullOrWhiteSpace(h.Text)) continue;
                        string role = (h.Role == "model" || h.Role == "assistant") ? "model" : "user";
                        contents.Add(new
                        {
                            role = role,
                            parts = new object[] { new { text = h.Text } }
                        });
                    }
                }

                // Mensaje actual del usuario
                contents.Add(new
                {
                    role = "user",
                    parts = new object[] { new { text = request.Message } }
                });

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts = new object[]
                        {
                            new { text = SystemPrompt }
                        }
                    },
                    contents = contents,
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 600
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(25);

                using var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                using var response = await client.PostAsync(url, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error en Gemini API [{StatusCode}]: {Response}", response.StatusCode, responseString);
                    return Ok(new ChatResponseDto
                    {
                        Success = false,
                        Reply = "Lo siento, hubo un problema al consultar a Gemini. Por favor verifica que tu API Key sea válida o intenta nuevamente en unos segundos.",
                        Error = responseString
                    });
                }

                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var contentElem) &&
                    contentElem.TryGetProperty("parts", out var partsElem) &&
                    partsElem.GetArrayLength() > 0 &&
                    partsElem[0].TryGetProperty("text", out var textElem))
                {
                    var replyText = textElem.GetString() ?? "";
                    return Ok(new ChatResponseDto
                    {
                        Success = true,
                        Reply = replyText.Trim()
                    });
                }

                return Ok(new ChatResponseDto
                {
                    Success = false,
                    Reply = "No pude generar una respuesta clara en este momento. ¿Podrías reformular tu consulta sobre tu mascota?"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comunicando con Gemini");
                return Ok(new ChatResponseDto
                {
                    Success = false,
                    Reply = "Ocurrió un error temporal al procesar tu consulta con VetBot. Por favor intenta nuevamente.",
                    Error = ex.Message
                });
            }
        }
    }
}
