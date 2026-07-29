using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [ApiController]
    [Route("api/whatsapp/webhook")]
    [EnableRateLimiting("WebhookPolicy")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly IHostEnvironment _environment;

        public WhatsAppWebhookController(IWhatsAppService whatsAppService, IHostEnvironment environment)
        {
            _whatsAppService = whatsAppService;
            _environment = environment;
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
            if (mode == "subscribe" && !string.IsNullOrEmpty(verifyToken) && verifyToken == _whatsAppService.Settings.VerifyToken)
            {
                return Ok(challenge);
            }

            return Unauthorized("Token de verificación inválido");
        }

        /// <summary>
        /// Endpoint de recepción de mensajes de WhatsApp (POST).
        /// Valida la firma HMAC SHA-256 de Meta (X-Hub-Signature-256) cuando está configurado AppSecret.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveMessage(
            [FromHeader(Name = "X-Hub-Signature-256")] string? signature)
        {
            try
            {
                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                string rawBody = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(rawBody))
                {
                    return BadRequest(new { status = "error", message = "Cuerpo de solicitud vacío" });
                }

                // Validación de Firma Meta (X-Hub-Signature-256)
                string appSecret = _whatsAppService.Settings.AppSecret;
                if (!string.IsNullOrEmpty(appSecret))
                {
                    if (string.IsNullOrEmpty(signature) || !IsValidHmacSignature(rawBody, signature, appSecret))
                    {
                        return Unauthorized(new { status = "error", message = "Firma HMAC de WhatsApp inválida" });
                    }
                }
                else if (!_environment.IsDevelopment())
                {
                    // En producción exige firma si no está deshabilitado explícitamente
                    return Unauthorized(new { status = "error", message = "AppSecret no configurado para validar webhooks de WhatsApp" });
                }

                using var payload = JsonDocument.Parse(rawBody);
                var root = payload.RootElement;

                string senderPhone = "";
                string messageContent = "";
                int? propertyId = null;

                // 1. Intentar extraer si es un payload estructurado de Meta WhatsApp Cloud API
                if (root.TryGetProperty("entry", out var entryArray) && entryArray.ValueKind == JsonValueKind.Array && entryArray.GetArrayLength() > 0)
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
                    if (root.TryGetProperty("senderPhone", out var phoneProp))
                    {
                        senderPhone = phoneProp.GetString() ?? "";
                    }
                    if (root.TryGetProperty("messageContent", out var msgProp))
                    {
                        messageContent = msgProp.GetString() ?? "";
                    }
                    if (root.TryGetProperty("propertyId", out var propIdProp) && propIdProp.ValueKind == JsonValueKind.Number)
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

        private static bool IsValidHmacSignature(string payload, string signatureHeader, string secret)
        {
            if (string.IsNullOrEmpty(signatureHeader) || !signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string expectedHash = signatureHeader["sha256=".Length..].Trim();
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(secretBytes);
            byte[] hashBytes = hmac.ComputeHash(payloadBytes);
            string computedHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHash),
                Encoding.UTF8.GetBytes(expectedHash.ToLowerInvariant())
            );
        }
    }
}
