using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.LeadPipeline;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Agent")]
    public class LeadsController : BaseApiController
    {
        private readonly ILeadPipelineService _leadService;

        public LeadsController(ILeadPipelineService leadService)
        {
            _leadService = leadService;
        }

        /// <summary>
        /// Obtiene todos los leads del agente autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LeadPipelineDto>))]
        public async Task<IActionResult> GetAll()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            var leads = await _leadService.GetAllByAgentAsync(agentId);
            return Ok(leads);
        }

        /// <summary>
        /// Obtiene los leads filtrados por etapa del pipeline.
        /// </summary>
        [HttpGet("stage/{stage}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LeadPipelineDto>))]
        public async Task<IActionResult> GetByStage(string stage)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            var leads = await _leadService.GetByStageAsync(agentId, stage);
            return Ok(leads);
        }

        /// <summary>
        /// Obtiene las estadísticas del pipeline Kanban del agente.
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeadPipelineStatsDto))]
        public async Task<IActionResult> GetStats()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            var stats = await _leadService.GetStatsAsync(agentId);
            return Ok(stats);
        }

        /// <summary>
        /// Obtiene un lead por su ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeadPipelineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var lead = await _leadService.GetByIdAsync(id);
            if (lead == null)
            {
                return NotFound(new { hasError = true, error = "Lead no encontrado." });
            }
            return Ok(lead);
        }

        /// <summary>
        /// Crea un nuevo lead en el pipeline.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LeadPipelineDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateLeadRequest request)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lead = await _leadService.CreateAsync(request, agentId);
            return CreatedAtAction(nameof(GetById), new { id = lead.Id }, lead);
        }

        /// <summary>
        /// Actualiza la información de un lead existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeadPipelineDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeadRequest request)
        {
            try
            {
                var lead = await _leadService.UpdateAsync(id, request);
                return Ok(lead);
            }
            catch (RealEstateApp.Core.Domain.Exceptions.NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Mueve un lead a una nueva etapa del pipeline Kanban.
        /// </summary>
        [HttpPatch("{id:int}/stage")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeadPipelineDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MoveToStage(int id, [FromBody] MoveStageRequest request)
        {
            try
            {
                var lead = await _leadService.MoveToStageAsync(id, request.Stage);
                return Ok(lead);
            }
            catch (RealEstateApp.Core.Domain.Exceptions.NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
            catch (RealEstateApp.Core.Domain.Exceptions.ValidationException ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el orden de un lead dentro de su columna.
        /// </summary>
        [HttpPatch("{id:int}/sort-order")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LeadPipelineDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSortOrder(int id, [FromBody] SortOrderRequest request)
        {
            try
            {
                var lead = await _leadService.UpdateSortOrderAsync(id, request.SortOrder);
                return Ok(lead);
            }
            catch (RealEstateApp.Core.Domain.Exceptions.NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un lead del pipeline.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _leadService.DeleteAsync(id);
                return NoContent();
            }
            catch (RealEstateApp.Core.Domain.Exceptions.NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
        }

        public class MoveStageRequest
        {
            public string Stage { get; set; } = string.Empty;
        }

        public class SortOrderRequest
        {
            public int SortOrder { get; set; }
        }
    }
}
