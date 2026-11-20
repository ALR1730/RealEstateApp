using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Common;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Presentation.WebApi.Models.Requests;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class PropertiesController : BaseApiController
    {
        private readonly IPropertyService _propertyService;
        private readonly IMapper _mapper;

        public PropertiesController(IPropertyService propertyService, IMapper mapper)
        {
            _propertyService = propertyService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene el catálogo paginado de propiedades con soporte para filtros combinados.
        /// Endpoint público.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<PropertyDto>))]
        public async Task<IActionResult> GetAsync([FromQuery] PropertyFilterViewModel? filter)
        {
            var isAgentOrAdmin = User?.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Agent") || User.IsInRole("Admin") || User.IsInRole("Developer"));
            var pagedResult = await _propertyService.GetWithFiltersPagedAsync(
                filter ?? new PropertyFilterViewModel(),
                includeSold: isAgentOrAdmin);

            var propertyDtos = _mapper.Map<List<PropertyDto>>(pagedResult.Items);
            return Ok(PagedResult<PropertyDto>.Create(propertyDtos, pagedResult.Page, pagedResult.PageSize, pagedResult.TotalCount));
        }

        /// <summary>
        /// Obtiene el detalle de una propiedad por su ID.
        /// Endpoint público.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var propertyViewModel = await _propertyService.GetByIdViewModel(id);

            var isAgentOrAdmin = User.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Agent") || User.IsInRole("Admin") || User.IsInRole("Developer"));
            if (propertyViewModel == null || (!isAgentOrAdmin && propertyViewModel.Status == PropertyStatus.Sold))
            {
                return NotFound(new { hasError = true, error = $"No se encontró ninguna propiedad con el ID {id}" });
            }

            var propertyDto = _mapper.Map<PropertyDto>(propertyViewModel);
            return Ok(propertyDto);
        }

        /// <summary>
        /// Obtiene el detalle de una propiedad por su código único de 6 dígitos.
        /// Endpoint público.
        /// </summary>
        [HttpGet("code/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCodeAsync(string code)
        {
            var propertyViewModel = await _propertyService.GetByCode(code);

            var isAgentOrAdmin = User.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Agent") || User.IsInRole("Admin") || User.IsInRole("Developer"));
            if (propertyViewModel == null || (!isAgentOrAdmin && propertyViewModel.Status == PropertyStatus.Sold))
            {
                return NotFound(new { hasError = true, error = $"No se encontró ninguna propiedad con el código '{code}'" });
            }

            var propertyDto = _mapper.Map<PropertyDto>(propertyViewModel);
            return Ok(propertyDto);
        }

        /// <summary>
        /// Obtiene las propiedades asignadas al agente actualmente autenticado.
        /// </summary>
        [Authorize(Roles = "Agent")]
        [HttpGet("my-properties")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyPropertiesAsync()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId))
            {
                return Unauthorized();
            }

            var properties = await _propertyService.GetByAgentId(agentId);
            var dtos = _mapper.Map<List<PropertyDto>>(properties);
            return Ok(dtos);
        }

        /// <summary>
        /// Registra una nueva propiedad en el catálogo (Requiere rol Agent).
        /// </summary>
        [Authorize(Roles = "Agent")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SavePropertyViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync([FromForm] SavePropertyFormRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var model = request.ToViewModel();
            var agentId = CurrentUserId;
            if (!string.IsNullOrEmpty(agentId))
            {
                model.AgentId = agentId;
            }

            var created = await _propertyService.Add(model);
            return StatusCode(StatusCodes.Status201Created, created);
        }

        /// <summary>
        /// Actualiza una propiedad existente (Requiere rol Agent o Admin).
        /// </summary>
        [Authorize(Roles = "Agent,Admin")]
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] SavePropertyFormRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { hasError = true, error = "El ID de la ruta no coincide con el modelo." });
            }

            var existing = await _propertyService.GetByIdViewModel(id);
            if (existing == null)
            {
                return NotFound();
            }

            var currentUserId = CurrentUserId;
            if (User.IsInRole("Agent") && existing.AgentId != currentUserId)
            {
                return Forbid();
            }

            await _propertyService.Update(request.ToViewModel(), id);
            return Ok(new { success = true, message = "Propiedad actualizada exitosamente." });
        }

        /// <summary>
        /// Elimina una propiedad (Requiere rol Agent o Admin).
        /// </summary>
        [Authorize(Roles = "Agent,Admin")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var existing = await _propertyService.GetByIdViewModel(id);
            if (existing == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Agent") && existing.AgentId != currentUserId)
            {
                return Forbid();
            }

            await _propertyService.Delete(id);
            return Ok(new { success = true, message = "Propiedad eliminada exitosamente." });
        }

        /// <summary>
        /// Alterna el estado destacado de una propiedad (Agent/Admin).
        /// </summary>
        [Authorize(Roles = "Agent,Admin")]
        [HttpPost("{id:int}/toggle-featured")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleFeaturedAsync(int id, [FromQuery] int durationDays = 30)
        {
            // Protección IDOR (OWASP API1): un Agente solo puede destacar sus propias propiedades.
            if (User.IsInRole("Agent"))
            {
                var property = await _propertyService.GetByIdViewModel(id);
                if (property == null)
                    return NotFound(new { hasError = true, error = $"No se encontró la propiedad con ID {id}." });

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (property.AgentId != currentUserId)
                    return Forbid();
            }

            await _propertyService.ToggleFeaturedAsync(id, durationDays);
            return Ok(new { success = true, message = "Estado destacado actualizado exitosamente." });
        }

        /// <summary>
        /// Reasigna una propiedad a un nuevo agente (Solo Administrador).
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/reassign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReassignPropertyAsync(int id, [FromBody] ReassignRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.NewAgentId))
            {
                return BadRequest(new { hasError = true, error = "Debe indicar el nuevo agente." });
            }

            await _propertyService.ReassignAgent(id, request.NewAgentId);
            return Ok(new { success = true, message = "Propiedad reasignada exitosamente." });
        }

        public class ReassignRequest
        {
            public string NewAgentId { get; set; } = string.Empty;
        }

        /// <summary>
        /// Obtiene el historial de cambios de precio de una propiedad.
        /// </summary>
        [HttpGet("{id:int}/price-history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPriceHistoryAsync(int id)
        {
            var property = await _propertyService.GetByIdViewModel(id);
            if (property == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ninguna propiedad con el ID {id}" });
            }

            var history = await _propertyService.GetPriceHistoryAsync(id);
            return Ok(history);
        }
    }
}
