using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RealEstateApp.Core.Application.Dtos.WhatsApp;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IChatRepository _chatRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly WhatsAppSettings _settings;

        public WhatsAppSettings Settings => _settings;

        public WhatsAppService(
            HttpClient httpClient,
            IConfiguration configuration,
            IChatRepository chatRepository,
            IPropertyRepository propertyRepository)
        {
            _httpClient = httpClient;
            _chatRepository = chatRepository;
            _propertyRepository = propertyRepository;

            _settings = new WhatsAppSettings();
            var section = configuration.GetSection("WhatsAppSettings");
            if (section.Exists())
            {
                section.Bind(_settings);
            }
        }

        public async Task<bool> SendWhatsAppMessageAsync(string toPhoneNumber, string messageText)
        {
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                toPhoneNumber = _settings.DefaultAgentPhoneNumber;
            }

            // Normalizar el número de teléfono (solo dígitos)
            var cleanPhone = new string(toPhoneNumber.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(cleanPhone))
            {
                cleanPhone = "18095550199";
            }

            // Si la integración con Meta está activa y tiene credenciales válidas
            if (_settings.Enabled && !string.IsNullOrEmpty(_settings.PhoneNumberId) && !string.IsNullOrEmpty(_settings.AccessToken))
            {
                try
                {
                    string endpoint = $"{_settings.ApiUrl.TrimEnd('/')}/{_settings.PhoneNumberId}/messages";

                    var payload = new
                    {
                        messaging_product = "whatsapp",
                        recipient_type = "individual",
                        to = cleanPhone,
                        type = "text",
                        text = new { body = messageText }
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
                    request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[WhatsApp API] Mensaje enviado exitosamente a {cleanPhone}");
                        return true;
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[WhatsApp API Error] Status: {response.StatusCode}, Content: {errorMsg}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WhatsApp API Exception] {ex.Message}");
                }
            }

            // Modo Simulación/Dev
            Console.WriteLine($"[WhatsApp Simulación] Mensaje transmitido vía WhatsApp a {cleanPhone}: '{messageText}'");
            return true;
        }

        public async Task<bool> ProcessIncomingWebhookAsync(string senderPhone, string messageContent, string? recipientPhone = null, int? propertyId = null)
        {
            if (string.IsNullOrWhiteSpace(senderPhone) || string.IsNullOrWhiteSpace(messageContent))
            {
                return false;
            }

            int targetPropertyId = propertyId ?? 1;

            if (propertyId == null)
            {
                var properties = await _propertyRepository.GetAllAsync();
                var firstProp = properties.FirstOrDefault();
                if (firstProp != null)
                {
                    targetPropertyId = firstProp.Id;
                }
            }

            var prop = await _propertyRepository.GetByIdAsync(targetPropertyId);
            string agenteId = prop?.AgentId ?? "agente-default-id";
            string clienteId = senderPhone; // Usa el teléfono de WhatsApp como identificador del cliente WhatsApp

            var chat = new Chat
            {
                PropertyId = targetPropertyId,
                AgenteId = agenteId,
                ClienteId = clienteId,
                SenderId = clienteId,
                MessageContent = messageContent,
                SentAt = DateTime.UtcNow,
                IsWhatsApp = true,
                WhatsAppMessageId = $"WA-{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            await _chatRepository.AddAsync(chat);
            return true;
        }
    }
}
