using System.Threading.Tasks;
using RealEstateApp.Core.Application.Dtos.WhatsApp;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IWhatsAppService
    {
        WhatsAppSettings Settings { get; }
        Task<bool> SendWhatsAppMessageAsync(string toPhoneNumber, string messageText);
        Task<bool> ProcessIncomingWebhookAsync(string senderPhone, string messageContent, string? recipientPhone = null, int? propertyId = null);
    }
}
