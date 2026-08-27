using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Extensions;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseApiController
    {
        private readonly IAgentService _agentService;
        private readonly IPropertyService _propertyService;
        private readonly IAccountService _accountService;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            IAgentService agentService,
            IPropertyService propertyService,
            IAccountService accountService,
            UserManager<IdentityUser> userManager)
        {
            _agentService = agentService;
            _propertyService = propertyService;
            _accountService = accountService;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtiene los KPIs ejecutivos consolidados para el Dashboard de Administración.
        /// </summary>
        [HttpGet("dashboard-kpis")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardKPIsAsync()
        {
            var properties = await _propertyService.GetAllViewModel();
            var agents = await _agentService.GetAllViewModelAsync();
            var clients = await _userManager.GetUsersInRoleAsync(Roles.Client.ToString());
            var developers = await _userManager.GetUsersInRoleAsync(Roles.Developer.ToString());

            var typeGroups = properties
                .GroupBy(p => p.PropertyTypeName ?? "Sin Categoría")
                .Select(g => new { typeName = g.Key, count = g.Count() })
                .ToList();

            var kpis = new
            {
                totalAvailableProperties = properties.Count(p => p.Status == PropertyStatus.Available),
                totalReservedProperties = properties.Count(p => p.Status == PropertyStatus.Reserved),
                totalSoldProperties = properties.Count(p => p.Status == PropertyStatus.Sold),
                totalActiveAgents = agents.Count(a => a.IsActive),
                totalInactiveAgents = agents.Count(a => !a.IsActive),
                totalClients = clients.Count,
                totalDevelopers = developers.Count,
                totalProperties = properties.Count,
                propertiesByType = typeGroups
            };

            return Ok(kpis);
        }

        /// <summary>
        /// Obtiene el listado completo de agentes inmobiliarios.
        /// </summary>
        [HttpGet("agents")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AgentViewModel>))]
        public async Task<IActionResult> GetAgentsAsync()
        {
            var agents = await _agentService.GetAllViewModelAsync();
            return Ok(agents);
        }

        /// <summary>
        /// Alterna el estado activo/inactivo de un agente inmobiliario.
        /// </summary>
        [HttpPatch("agents/{agentId}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleAgentStatusAsync(string agentId, [FromBody] bool isActive)
        {
            try
            {
                await _agentService.ChangeStatusAsync(agentId, isActive);
                return Ok(new { success = true, message = $"Agente {(isActive ? "activado" : "inactivado")} exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Reasigna todas las propiedades de un agente a otro.
        /// </summary>
        [HttpPost("agents/reassign-properties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReassignPropertiesAsync([FromQuery] string sourceAgentId, [FromQuery] string targetAgentId)
        {
            try
            {
                var properties = await _propertyService.GetByAgentId(sourceAgentId);
                foreach (var prop in properties)
                {
                    await _propertyService.ReassignAgent(prop.Id, targetAgentId);
                }
                return Ok(new { success = true, message = $"Se reasignaron {properties.Count} propiedades exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina físicamente en cascada un agente y sus propiedades.
        /// </summary>
        [HttpDelete("agents/{agentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAgentAsync(string agentId)
        {
            try
            {
                await _agentService.DeleteAgentCascadeAsync(agentId);
                return Ok(new { success = true, message = "El agente y sus registros asociados fueron eliminados en cascada." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el listado de usuarios Administradores.
        /// </summary>
        [HttpGet("admins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminsAsync()
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync(Roles.Admin.ToString());
            var result = adminUsers.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                isActive = u.IsActiveUser()
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Alterna el estado activo/inactivo de un Administrador.
        /// </summary>
        [HttpPatch("admins/{userId}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleAdminStatusAsync(string userId, [FromBody] bool isActive)
        {
            try
            {
                await _accountService.ChangeUserStatusAsync(userId, isActive);
                return Ok(new { success = true, message = $"Administrador {(isActive ? "activado" : "inactivado")} exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el listado de usuarios Desarrolladores.
        /// </summary>
        [HttpGet("developers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDevelopersAsync()
        {
            var devUsers = await _userManager.GetUsersInRoleAsync(Roles.Developer.ToString());
            var result = devUsers.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.PhoneNumber,
                isActive = u.IsActiveUser()
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Alterna el estado activo/inactivo de un Desarrollador.
        /// </summary>
        [HttpPatch("developers/{userId}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleDeveloperStatusAsync(string userId, [FromBody] bool isActive)
        {
            try
            {
                await _accountService.ChangeUserStatusAsync(userId, isActive);
                return Ok(new { success = true, message = $"Desarrollador {(isActive ? "activado" : "inactivado")} exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuevo usuario Desarrollador.
        /// </summary>
        [HttpPost("developers")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDeveloperAsync([FromBody] RegisterRequest request)
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
        /// Actualiza un usuario Desarrollador existente.
        /// </summary>
        [HttpPut("developers/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDeveloperAsync(string userId, [FromBody] RegisterRequest request)
        {
            var existing = await _userManager.FindByIdAsync(userId);
            if (existing == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró el desarrollador con ID {userId}" });
            }

            existing.UserName = request.Email;
            existing.Email = request.Email;
            var result = await _userManager.UpdateAsync(existing);

            if (!result.Succeeded)
            {
                return BadRequest(new { hasError = true, error = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            return Ok(new { success = true, message = "Desarrollador actualizado exitosamente." });
        }

        /// <summary>
        /// Crea un nuevo usuario Administrador.
        /// </summary>
        [HttpPost("admins")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RegisterResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAdminAsync([FromBody] RegisterRequest request)
        {
            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _accountService.RegisterUserAsync(request, Roles.Admin.ToString(), origin);

            if (response.HasError)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
