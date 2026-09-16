using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using RealEstateApp.Core.Application.Dtos.WhatsApp;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly WhatsAppSettings _settings;

        public string DefaultAgentPhoneNumber => _settings.DefaultAgentPhoneNumber;

        public WhatsAppService(IConfiguration configuration)
        {
            _settings = new WhatsAppSettings();
            var section = configuration.GetSection("WhatsAppSettings");
            if (section.Exists())
            {
                section.Bind(_settings);
            }
        }

        public string GenerateWhatsAppUrl(string phoneNumber, string messageText)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                phoneNumber = _settings.DefaultAgentPhoneNumber;
            }

            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(cleanPhone))
            {
                cleanPhone = new string(_settings.DefaultAgentPhoneNumber.Where(char.IsDigit).ToArray());
            }

            var encodedText = Uri.EscapeDataString(messageText ?? string.Empty);
            return $"https://wa.me/{cleanPhone}?text={encodedText}";
        }

        public string GenerateWhatsAppUrlForProperty(string phoneNumber, string propertyCode, string propertyType, string? additionalMessage = null)
        {
            var message = $"Hola, estoy interesado en el inmueble código {propertyCode} ({propertyType}) publicado en RealEstateApp.";
            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                message += $"\n\n{additionalMessage}";
            }
            return GenerateWhatsAppUrl(phoneNumber, message);
        }
    }
}
