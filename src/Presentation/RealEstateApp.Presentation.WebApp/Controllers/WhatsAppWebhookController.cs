using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [ApiController]
    [Route("api/whatsapp/webhook")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;

        public WhatsAppWebhookController(IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        /// <summary>
        /// Endpoint de verificación para Meta WhatsApp Cloud API (GET).
        /// </summary>
        [HttpGet]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string? mode,
            [FromQuery(Name = "hub.verify_token")] string? verifyToken,
            [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            if (mode == "subscribe" && verifyToken == _whatsAppService.Settings.VerifyToken)
            {
                return Ok(challenge);
            }

            return Unauthorized("Token de verificación inválido");
        }

        /// <summary>
        /// Endpoint de recepción de mensajes de WhatsApp (POST).
        /// Soporta payloads oficiales de Meta WhatsApp Cloud API y peticiones del Simulador Local.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveMessage([FromBody] JsonElement payload)
        {
            try
            {
                string senderPhone = "";
                string messageContent = "";
                int? propertyId = null;

                // 1. Intentar extraer si es un payload estructurado de Meta WhatsApp Cloud API
                if (payload.TryGetProperty("entry", out var entryArray) && entryArray.ValueKind == JsonValueKind.Array && entryArray.GetArrayLength() > 0)
                {
                    var entry = entryArray[0];
                    if (entry.TryGetProperty("changes", out var changesArray) && changesArray.ValueKind == JsonValueKind.Array && changesArray.GetArrayLength() > 0)
                    {
                        var value = changesArray[0].GetProperty("value");
                        if (value.TryGetProperty("messages", out var messagesArray) && messagesArray.ValueKind == JsonValueKind.Array && messagesArray.GetArrayLength() > 0)
                        {
                            var msg = messagesArray[0];
                            if (msg.TryGetProperty("from", out var fromProp))
                            {
                                senderPhone = fromProp.GetString() ?? "";
                            }
                            if (msg.TryGetProperty("text", out var textProp) && textProp.TryGetProperty("body", out var bodyProp))
                            {
                                messageContent = bodyProp.GetString() ?? "";
                            }
                        }
                    }
                }

                // 2. Si no es de Meta, intentar leer payload directo de simulación
                if (string.IsNullOrEmpty(senderPhone) || string.IsNullOrEmpty(messageContent))
                {
                    if (payload.TryGetProperty("senderPhone", out var phoneProp))
                    {
                        senderPhone = phoneProp.GetString() ?? "";
                    }
                    if (payload.TryGetProperty("messageContent", out var msgProp))
                    {
                        messageContent = msgProp.GetString() ?? "";
                    }
                    if (payload.TryGetProperty("propertyId", out var propIdProp) && propIdProp.ValueKind == JsonValueKind.Number)
                    {
                        propertyId = propIdProp.GetInt32();
                    }
                }

                if (string.IsNullOrWhiteSpace(senderPhone) || string.IsNullOrWhiteSpace(messageContent))
                {
                    return BadRequest(new { status = "error", message = "Faltan datos del remitente o contenido del mensaje" });
                }

                bool result = await _whatsAppService.ProcessIncomingWebhookAsync(senderPhone, messageContent, null, propertyId);

                if (result)
                {
                    return Ok(new { status = "success", message = "Mensaje de WhatsApp procesado e integrado al chat de la aplicación" });
                }

                return StatusCode(500, new { status = "error", message = "No se pudo procesar el mensaje" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
