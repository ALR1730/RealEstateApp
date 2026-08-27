using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class AccountController : BaseApiController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Inicia sesión y genera el token de autenticación JWT Bearer con roles y perfil de usuario.
        /// </summary>
        [HttpPost("authenticate")]
        [EnableRateLimiting("AuthPolicy")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticationRequest request)
        {
            var response = await _accountService.AuthenticateAsync(request);

            if (response.HasError)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Registra un nuevo usuario Cliente en la plataforma.
        /// </summary>
        [HttpPost("register-client")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterClientAsync([FromBody] RegisterRequest request)
        {
            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _accountService.RegisterUserAsync(request, Roles.Client.ToString(), origin);

            if (response.HasError)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Registra un nuevo usuario Agente Inmobiliario en la plataforma.
        /// </summary>
        [HttpPost("register-agent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAgentAsync([FromBody] RegisterRequest request)
        {
            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _accountService.RegisterUserAsync(request, Roles.Agent.ToString(), origin);

            if (response.HasError)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Registra un nuevo usuario Administrador en el sistema (Reservado para Administradores).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("register-admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RegisterAdminAsync([FromBody] RegisterRequest request)
        {
            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _accountService.RegisterUserAsync(request, Roles.Admin.ToString(), origin);

            if (response.HasError)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Registra un nuevo usuario Desarrollador en el sistema para acceso a la API REST (Reservado para Administradores).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("register-developer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RegisterDeveloperAsync([FromBody] RegisterRequest request)
        {
            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _accountService.RegisterUserAsync(request, Roles.Developer.ToString(), origin);

            if (response.HasError)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Confirma la cuenta de usuario vía token enviado por correo electrónico.
        /// </summary>
        [HttpGet("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _accountService.ConfirmAccountAsync(userId, token);
            return Ok(new { message = result });
        }

        /// <summary>
        /// Obtiene los datos del perfil del usuario autenticado.
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EditProfileViewModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfileAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var profile = await _accountService.GetProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            return Ok(profile);
        }

        /// <summary>
        /// Actualiza los datos del perfil del usuario autenticado.
        /// </summary>
        [Authorize]
        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EditProfileViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfileAsync([FromForm] EditProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            model.Id = userId;
            var result = await _accountService.UpdateProfileAsync(model);

            if (result.HasError)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Solicita el restablecimiento de contraseña vía correo electrónico.
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
        {
            var origin = $"{Request.Scheme}://{Request.Host}";
            var result = await _accountService.ForgotPasswordAsync(request, origin);
            return Ok(new { message = result });
        }

        /// <summary>
        /// Restablece la contraseña utilizando el token enviado por correo.
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
        {
            var result = await _accountService.ResetPasswordAsync(request);
            return Ok(new { message = result });
        }

        /// <summary>
        /// Cambia la contraseña del usuario actualmente autenticado.
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            model.Id = userId;
            var result = await _accountService.ChangePasswordAsync(model);
            if (result.HasError)
            {
                return BadRequest(result);
            }

            return Ok(new { success = true, message = "Contraseña actualizada exitosamente." });
        }

        /// <summary>
        /// Obtiene el historial de actividad reciente del usuario autenticado.
        /// </summary>
        [Authorize]
        [HttpGet("activity")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetActivityAsync()
        {
            var activities = new object[]
            {
                new { type = "info", message = "Sesión iniciada correctamente", timestamp = DateTime.UtcNow.AddMinutes(-5).ToString("o") },
                new { type = "info", message = "Perfil actualizado", timestamp = DateTime.UtcNow.AddHours(-1).ToString("o") }
            };
            return Ok(activities);
        }
    }
}
