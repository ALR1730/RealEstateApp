namespace RealEstateApp.Core.Application.Dtos.WhatsApp
{
    public class WhatsAppSettings
    {
        public bool Enabled { get; set; } = true;
        public string PhoneNumberId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string VerifyToken { get; set; } = "realestateapp_whatsapp_verify_token";
        public string AppSecret { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://graph.facebook.com/v18.0/";
        public string DefaultAgentPhoneNumber { get; set; } = "+18095550199";
    }
}
