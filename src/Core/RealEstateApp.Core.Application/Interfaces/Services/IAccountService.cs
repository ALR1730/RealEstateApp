using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Account;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de servicio de cuenta de usuario (Identity).
    /// Implementado en la capa de Infraestructura Persistence.
    /// </summary>
    public interface IAccountService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<RegisterResponse> RegisterBasicUserAsync(RegisterRequest request);
        Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string role, string? origin = null);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task ChangeUserStatusAsync(string userId, bool isActive);
        Task SignOutAsync();
    }
}
