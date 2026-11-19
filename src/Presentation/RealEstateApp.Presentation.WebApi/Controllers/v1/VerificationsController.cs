using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Presentation.WebApi.Models.Requests;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize]
    public class VerificationsController : BaseApiController
    {
        private readonly IAgentVerificationService _verificationService;

        public VerificationsController(IAgentVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        /// <summary>
        /// Obtiene todas las solicitudes de verificación KYC (Solo Administrador).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var list = await _verificationService.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Obtiene el estado de verificación del agente autenticado.
        /// </summary>
        [HttpGet("my-status")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyStatus()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var status = await _verificationService.GetByAgentIdAsync(userId);
            return Ok(status);
        }

        /// <summary>
        /// Envía solicitud de verificación de identidad KYC (Cédula y fotos) para el agente.
        /// </summary>
        [HttpPost("submit")]
        [Authorize(Roles = "Agent")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Submit([FromForm] SubmitVerificationFormRequest request)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var vm = request.ToViewModel(userId);
            var success = await _verificationService.SubmitVerificationAsync(vm);
            if (!success)
            {
                return BadRequest(new { hasError = true, error = "No se pudo procesar la solicitud de verificación. Verifica los datos y archivos." });
            }

            return Ok(new { success = true, message = "Solicitud de verificación enviada exitosamente para revisión." });
        }

        /// <summary>
        /// Aprueba la verificación de un agente (Solo Administrador).
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Approve(int id)
        {
            var adminId = CurrentUserId ?? "Admin";
            var success = await _verificationService.ReviewVerificationAsync(id, true, null, adminId);
            if (!success)
            {
                return BadRequest(new { hasError = true, error = "No se pudo aprobar la verificación." });
            }

            return Ok(new { success = true, message = "Verificación de agente aprobada con éxito." });
        }

        /// <summary>
        /// Rechaza la verificación de un agente indicando motivo (Solo Administrador).
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectVerificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Reason))
            {
                return BadRequest(new { hasError = true, error = "Debe indicar el motivo del rechazo." });
            }

            var adminId = CurrentUserId ?? "Admin";
            var success = await _verificationService.ReviewVerificationAsync(id, false, request.Reason, adminId);
            if (!success)
            {
                return BadRequest(new { hasError = true, error = "No se pudo rechazar la verificación." });
            }

            return Ok(new { success = true, message = "Solicitud de verificación marcada como rechazada." });
        }

        public class RejectVerificationRequest
        {
            public string Reason { get; set; } = string.Empty;
        }
    }
}
