using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Agent;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class AgentsController : BaseApiController
    {
        private readonly IAgentService _agentService;

        public AgentsController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        /// <summary>
        /// Obtiene el listado público de todos los agentes inmobiliarios.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AgentDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAsync()
        {
            var agents = await _agentService.GetAllDtoAsync();

            if (agents == null || agents.Count == 0)
            {
                return Ok(new List<AgentDto>());
            }

            return Ok(agents);
        }

        /// <summary>
        /// Obtiene la información detallada de un agente por su ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AgentDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            var agent = await _agentService.GetByIdDtoAsync(id);

            if (agent == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún agente con el ID '{id}'" });
            }

            return Ok(agent);
        }

        /// <summary>
        /// Obtiene el portafolio de propiedades asociadas a un agente.
        /// </summary>
        [HttpGet("{id}/properties")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AgentPropertyDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAgentPropertiesAsync(string id)
        {
            var agent = await _agentService.GetByIdDtoAsync(id);

            if (agent == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún agente con el ID '{id}'" });
            }

            return Ok(agent.Properties ?? new List<AgentPropertyDto>());
        }

        /// <summary>
        /// Activa o inactiva la cuenta de un agente inmobiliario (Reservado para Administradores).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/change-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeStatusAsync(string id, [FromBody] ChangeAgentStatusRequest request)
        {
            try
            {
                await _agentService.ChangeStatusAsync(id, request.IsActive);
                var statusText = request.IsActive ? "activado" : "inactivado";
                return Ok(new { hasError = false, message = $"El agente fue {statusText} exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Objeto de solicitud para cambiar el estado activo de un agente.
    /// </summary>
    public class ChangeAgentStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
