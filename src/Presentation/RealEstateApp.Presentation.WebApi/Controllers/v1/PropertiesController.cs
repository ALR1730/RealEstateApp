using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;

using Microsoft.AspNetCore.Authorization;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Developer,Admin")]
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
        /// Obtiene el catálogo de propiedades con soporte para filtros combinados.
        /// End-point público.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PropertyDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAsync([FromQuery] PropertyFilterViewModel filter)
        {
            var propertyViewModels = await _propertyService.GetAllWithFilters(filter);

            var isAgentOrAdmin = User.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Agent") || User.IsInRole("Admin") || User.IsInRole("Developer"));
            if (!isAgentOrAdmin && propertyViewModels != null)
            {
                propertyViewModels = propertyViewModels.Where(p => p.Status != PropertyStatus.Sold).ToList();
            }

            if (propertyViewModels == null || propertyViewModels.Count == 0)
            {
                return NoContent();
            }

            var propertyDtos = _mapper.Map<List<PropertyDto>>(propertyViewModels);
            return Ok(propertyDtos);
        }

        /// <summary>
        /// Obtiene el detalle de una propiedad por su ID.
        /// End-point público.
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
        /// End-point público.
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
    }
}
