using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class CommissionsController : BaseApiController
    {
        private readonly ICommissionService _commissionService;

        public CommissionsController(ICommissionService commissionService)
        {
            _commissionService = commissionService;
        }

        /// <summary>
        /// Comisiones del agente autenticado.
        /// </summary>
        [HttpGet("my-commissions")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyCommissions()
        {
            var agentId = CurrentUserId;
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            var commissions = await _commissionService.GetByAgentAsync(agentId);
            return Ok(commissions);
        }

        /// <summary>
        /// Resumen de comisiones del agente autenticado (pendientes y pagadas).
        /// </summary>
        [HttpGet("my-commissions/summary")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMySummary()
        {
            var agentId = CurrentUserId;
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            var summary = await _commissionService.GetAgentSummaryAsync(agentId);
            return Ok(summary);
        }

        /// <summary>
        /// Todas las comisiones del sistema (administrador).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var commissions = await _commissionService.GetAllAsync();
            return Ok(commissions);
        }

        /// <summary>
        /// Marca una comisión como pagada (administrador).
        /// </summary>
        [HttpPatch("{commissionId:int}/pay")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsPaid(int commissionId)
        {
            await _commissionService.MarkAsPaidAsync(commissionId);
            return Ok(new { success = true, message = "Comisión marcada como pagada." });
        }
    }
}
