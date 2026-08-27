using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.PropertyType;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.PropertyType;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class PropertyTypesController : BaseApiController
    {
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly IMapper _mapper;

        public PropertyTypesController(IPropertyTypeService propertyTypeService, IMapper mapper)
        {
            _propertyTypeService = propertyTypeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene el listado de todos los tipos de propiedad.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PropertyTypeDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAsync()
        {
            var list = await _propertyTypeService.GetAllViewModel();

            if (list == null || list.Count == 0)
            {
                return Ok(new List<PropertyTypeDto>());
            }

            var dtos = list.Select(x => new PropertyTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                PropertiesCount = x.PropertiesCount
            }).ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un tipo de propiedad por su ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyTypeDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var saveVm = await _propertyTypeService.GetByIdSaveViewModel(id);

            if (saveVm == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún tipo de propiedad con el ID {id}" });
            }

            var dto = new PropertyTypeDto
            {
                Id = saveVm.Id,
                Name = saveVm.Name,
                Description = saveVm.Description
            };
            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo tipo de propiedad (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PropertyTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostAsync([FromBody] SavePropertyTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdVm = await _propertyTypeService.Add(vm);
            var dto = new PropertyTypeDto
            {
                Id = createdVm.Id,
                Name = createdVm.Name,
                Description = createdVm.Description
            };
            return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
        }

        /// <summary>
        /// Actualiza un tipo de propiedad existente (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropertyTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] SavePropertyTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _propertyTypeService.GetByIdSaveViewModel(id);
            if (existing == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún tipo de propiedad con el ID {id}" });
            }

            vm.Id = id;
            await _propertyTypeService.Update(vm, id);
            var updatedVm = await _propertyTypeService.GetByIdSaveViewModel(id);
            var dto = new PropertyTypeDto
            {
                Id = updatedVm?.Id ?? id,
                Name = updatedVm?.Name ?? vm.Name,
                Description = updatedVm?.Description ?? vm.Description
            };

            return Ok(dto);
        }

        /// <summary>
        /// Elimina un tipo de propiedad (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var existing = await _propertyTypeService.GetByIdSaveViewModel(id);
            if (existing == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún tipo de propiedad con el ID {id}" });
            }

            await _propertyTypeService.Delete(id);
            return NoContent();
        }
    }
}
