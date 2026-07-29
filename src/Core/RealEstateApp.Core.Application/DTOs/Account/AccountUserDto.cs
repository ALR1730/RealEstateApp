using System.Collections.Generic;

namespace RealEstateApp.Core.Application.DTOs.Account
{
    /// <summary>
    /// DTO para transferir información básica de cuentas de usuario entre capas sin exponer entidades de Identity.
    /// </summary>
    public class AccountUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
