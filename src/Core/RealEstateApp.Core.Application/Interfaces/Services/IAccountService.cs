using RealEstateApp.Core.Application.DTOs.Account;
using System.Threading.Tasks;

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
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task SignOutAsync();
    }
}
