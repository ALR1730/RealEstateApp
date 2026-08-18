using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.ViewModels.Account;

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
        Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string role, string? origin = null, string? route = null);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task ChangeUserStatusAsync(string userId, bool isActive);
        Task SignOutAsync();
        Task<EditProfileViewModel?> GetProfileAsync(string userId);
        Task<EditProfileViewModel> UpdateProfileAsync(EditProfileViewModel model);
        Task<List<AccountUserDto>> GetUsersInRoleAsync(string roleName);
        Task<AccountUserDto?> GetUserByIdAsync(string id);
        Task<Dictionary<string, AccountUserDto>> GetUsersByIdsAsync(IEnumerable<string> ids);
        Task DeleteUserAsync(string userId);
    }
}
