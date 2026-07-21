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
using RealEstateApp.Core.Application.Interfaces.Services;
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
        private readonly JWTSettings _jwtSettings;

        public AccountService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService,
            IOptions<JWTSettings> jwtSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
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

            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
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

        public async Task<RegisterResponse> RegisterUserAsync(RegisterRequest request, string role, string? origin = null)
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
                var route = "api/v1/account/confirm-email";
                var verificationUri = $"{origin}/{route}?userId={user.Id}&token={encodedToken}";

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
    }
}
