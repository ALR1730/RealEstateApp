namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IWhatsAppService
    {
        string DefaultAgentPhoneNumber { get; }
        string GenerateWhatsAppUrl(string phoneNumber, string messageText);
        string GenerateWhatsAppUrlForProperty(string phoneNumber, string propertyCode, string propertyType, string? additionalMessage = null);
    }
}
