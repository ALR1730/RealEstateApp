namespace RealEstateApp.Core.Application.DTOs.Account
{
    public class RegisterResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
