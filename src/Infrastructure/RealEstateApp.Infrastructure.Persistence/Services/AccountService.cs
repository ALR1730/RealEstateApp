using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Extensions;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Settings;

namespace RealEstateApp.Infrastructure.Persistence.Services
{
    /// <summary>
    /// Servicio de gestión de cuentas de usuario basado en ASP.NET Core Identity y JWT.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IFileStorageService _fileStorageService;
        private readonly JWTSettings _jwtSettings;

        public AccountService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IFileStorageService fileStorageService,
            IOptions<JWTSettings> jwtSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _fileStorageService = fileStorageService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            var response = new AuthenticationResponse();

            var user = await _userManager.FindByEmailAsync(request.Email)
                       ?? await _userManager.FindByNameAsync(request.Email);

            if (user == null)
            {
                response.HasError = true;
                response.Error = $"No se encontró ninguna cuenta registrada con el correo/usuario: '{request.Email}'";
                return response;
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                response.HasError = true;
                response.Error = "Credenciales incorrectas para el usuario especificado";
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Error = $"La cuenta para '{request.Email}' no ha sido confirmada por correo electrónico";
                return response;
            }

            if (!user.IsActiveUser())
            {
                response.HasError = true;
                response.Error = $"La cuenta '{request.Email}' se encuentra inactivada o bloqueada por el administrador";
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var jwtToken = await GenerateJwtTokenAsync(user);

            response.Id = user.Id;
            response.UserName = user.UserName ?? string.Empty;
            response.Email = user.Email ?? string.Empty;
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            response.HasError = false;

            return response;
        }

        public async Task<RegisterResponse> RegisterBasicUserAsync(RegisterRequest request)
        {
            return await RegisterUserAsync(request, Roles.Client.ToString());
        }

        public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string role, string? origin = null, string? route = null)
        {
            var response = new RegisterResponse();

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Error = $"El correo electrónico '{request.Email}' ya está registrado";
                return response;
            }

            var userWithSameUsername = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUsername != null)
            {
                response.HasError = true;
                response.Error = $"El nombre de usuario '{request.UserName}' ya está en uso";
                return response;
            }

            var user = new IdentityUser
            {
                Email = request.Email,
                UserName = request.UserName,
                PhoneNumber = request.Phone,
                EmailConfirmed = role == Roles.Admin.ToString() || role == Roles.Developer.ToString() // Admins y Developers nacen confirmados o según flujo
            };

            // Para clientes o agentes no autorizados por defecto, requieren confirmación o activación
            if (role == Roles.Client.ToString())
            {
                user.EmailConfirmed = false;
            }
            else if (role == Roles.Agent.ToString())
            {
                user.EmailConfirmed = true; // Email verificado, pero inactivo por lockout hasta aprobación admin
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue; // Inactivo por defecto
            }

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = string.Join(", ", result.Errors.Select(e => e.Description));
                return response;
            }

            // Asignar rol
            if (await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            // Enviar correo de activación si corresponde
            if (role == Roles.Client.ToString() && !string.IsNullOrWhiteSpace(origin))
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var targetRoute = !string.IsNullOrWhiteSpace(route)
                    ? route
                    : (origin.Contains("api", StringComparison.OrdinalIgnoreCase)
                        ? "api/v1/account/confirm-email"
                        : "Account/ConfirmEmail");
                var verificationUri = $"{origin}/{targetRoute}?userId={user.Id}&token={encodedToken}";

                await _emailService.SendAsync(
                    user.Email,
                    "Activación de Cuenta - RealEstateApp",
                    $"Por favor confirme su cuenta haciendo clic en el siguiente enlace: <a href='{verificationUri}'>Activar Cuenta</a>"
                );
            }

            response.Id = user.Id;
            response.UserName = user.UserName;
            response.Email = user.Email;
            response.HasError = false;

            return response;
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "No existe ningún usuario registrado con este ID";
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return $"Cuenta confirmada exitosamente para {user.Email}. Ya puede iniciar sesión.";
            }

            return $"Error al confirmar la cuenta para {user.Email}";
        }

        public async Task ChangeUserStatusAsync(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"No se encontró el usuario con ID '{userId}'");
            }

            if (isActive)
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            await _userManager.UpdateAsync(user);
        }

        public Task SignOutAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<EditProfileViewModel?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var claims = await _userManager.GetClaimsAsync(user);
            var firstNameClaim = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value ?? user.UserName ?? string.Empty;
            var lastNameClaim = claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? string.Empty;
            var profilePictureClaim = claims.FirstOrDefault(c => c.Type == "ProfilePicture")?.Value ?? string.Empty;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? string.Empty;

            return new EditProfileViewModel
            {
                Id = user.Id,
                FirstName = firstNameClaim,
                LastName = lastNameClaim,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber,
                ProfilePictureUrl = profilePictureClaim,
                Role = role
            };
        }

        public async Task<EditProfileViewModel> UpdateProfileAsync(EditProfileViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                model.HasError = true;
                model.ErrorMessage = "El usuario especificado no existe.";
                return model;
            }

            // 1. Cambio opcional de contraseña
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    model.HasError = true;
                    model.ErrorMessage = "Debe ingresar su contraseña actual para poder establecer una nueva contraseña.";
                    return model;
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    model.HasError = true;
                    model.ErrorMessage = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
                    return model;
                }
            }

            // 2. Carga y actualización de foto de perfil
            if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
            {
                var existingClaims = await _userManager.GetClaimsAsync(user);
                var oldPicture = existingClaims.FirstOrDefault(c => c.Type == "ProfilePicture")?.Value;
                if (!string.IsNullOrEmpty(oldPicture))
                {
                    await _fileStorageService.DeleteFileAsync(oldPicture, "profiles");
                }

                using var stream = model.ProfilePictureFile.OpenReadStream();
                model.ProfilePictureUrl = await _fileStorageService.UploadFileAsync(stream, model.ProfilePictureFile.FileName, "profiles");
            }

            // 3. Actualizar propiedades principales de IdentityUser
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PhoneNumber = model.Phone;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                model.HasError = true;
                model.ErrorMessage = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return model;
            }

            // 4. Actualizar o agregar Claims
            var currentClaims = await _userManager.GetClaimsAsync(user);
            await UpdateUserClaim(user, currentClaims, "FirstName", model.FirstName);
            await UpdateUserClaim(user, currentClaims, "LastName", model.LastName);
            if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
            {
                await UpdateUserClaim(user, currentClaims, "ProfilePicture", model.ProfilePictureUrl);
            }

            model.HasError = false;
            return model;
        }

        private async Task UpdateUserClaim(IdentityUser user, IList<Claim> existingClaims, string claimType, string value)
        {
            var existingClaim = existingClaims.FirstOrDefault(c => c.Type == claimType);
            if (existingClaim != null)
            {
                await _userManager.ReplaceClaimAsync(user, existingClaim, new Claim(claimType, value));
            }
            else
            {
                await _userManager.AddClaimAsync(user, new Claim(claimType, value));
            }
        }

        private async Task<JwtSecurityToken> GenerateJwtTokenAsync(IdentityUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials
            );

            return jwtToken;
        }

        public async Task<List<AccountUserDto>> GetUsersInRoleAsync(string roleName)
        {
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            var userDtos = new List<AccountUserDto>();

            foreach (var user in users)
            {
                var isActive = user.IsActiveUser();
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);

                var firstNameClaim = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
                var lastNameClaim = claims.FirstOrDefault(c => c.Type == "LastName")?.Value;
                var profilePictureClaim = claims.FirstOrDefault(c => c.Type == "ProfilePicture")?.Value;

                userDtos.Add(new AccountUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    FirstName = !string.IsNullOrWhiteSpace(firstNameClaim) ? firstNameClaim : (user.UserName ?? string.Empty),
                    LastName = lastNameClaim ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePictureUrl = profilePictureClaim,
                    IsActive = isActive,
                    Roles = roles.ToList()
                });
            }

            return userDtos;
        }

        public async Task<AccountUserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var isActive = user.IsActiveUser();
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            var firstNameClaim = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
            var lastNameClaim = claims.FirstOrDefault(c => c.Type == "LastName")?.Value;
            var profilePictureClaim = claims.FirstOrDefault(c => c.Type == "ProfilePicture")?.Value;

            return new AccountUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FirstName = !string.IsNullOrWhiteSpace(firstNameClaim) ? firstNameClaim : (user.UserName ?? string.Empty),
                LastName = lastNameClaim ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = profilePictureClaim,
                IsActive = isActive,
                Roles = roles.ToList()
            };
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"El usuario con ID '{userId}' no existe");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new Exception($"Error al eliminar el usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
