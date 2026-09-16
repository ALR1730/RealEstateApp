using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Appointment;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize]
    public class AppointmentsController : BaseApiController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        /// <summary>
        /// Obtiene las citas del usuario autenticado (Cliente o Agente).
        /// </summary>
        [HttpGet("my-appointments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AppointmentViewModel>))]
        public async Task<IActionResult> GetMyAppointmentsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (User.IsInRole("Agent"))
            {
                var appointments = await _appointmentService.GetByAgentIdAsync(userId);
                return Ok(appointments);
            }
            else
            {
                var appointments = await _appointmentService.GetByClienteIdAsync(userId);
                return Ok(appointments);
            }
        }

        /// <summary>
        /// Solicita una nueva cita para visitar un inmueble (Cliente).
        /// </summary>
        [Authorize(Roles = "Client")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AppointmentViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RequestAppointmentAsync([FromBody] SaveAppointmentViewModel model)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var created = await _appointmentService.RequestAppointmentAsync(model, clientId);
                return Ok(created);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Confirma una cita programada (Agente).
        /// </summary>
        [Authorize(Roles = "Agent")]
        [HttpPatch("{id:int}/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmAppointmentAsync(int id, [FromBody] string? notes)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            await _appointmentService.ConfirmAppointmentAsync(id, agentId, notes);
            return Ok(new { success = true, message = "Cita confirmada exitosamente." });
        }

        /// <summary>
        /// Cancela una cita (Cliente o Agente).
        /// </summary>
        [HttpPatch("{id:int}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelAppointmentAsync(int id, [FromBody] string? reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _appointmentService.CancelAppointmentAsync(id, userId, reason);
            return Ok(new { success = true, message = "Cita cancelada." });
        }
    }
}
