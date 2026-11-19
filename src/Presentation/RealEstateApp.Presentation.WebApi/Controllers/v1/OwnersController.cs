using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Presentation.WebApi.Models.Requests;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Owner")]
    public class OwnersController : BaseApiController
    {
        private readonly IPropertyService _propertyService;
        private readonly IMapper _mapper;

        public OwnersController(
            IPropertyService propertyService,
            IMapper mapper)
        {
            _propertyService = propertyService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene los inmuebles publicados por el propietario directo.
        /// </summary>
        [HttpGet("my-properties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProperties()
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var summary = await _propertyService.GetOwnerPropertiesSummaryAsync(userId);
            return Ok(new
            {
                maxAllowed = summary.MaxAllowed,
                currentCount = summary.CurrentCount,
                canCreate = summary.CanCreate,
                properties = summary.Properties
            });
        }

        /// <summary>
        /// Publica un inmueble directo como propietario.
        /// </summary>
        [HttpPost("properties")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProperty([FromForm] SavePropertyFormRequest request)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var created = await _propertyService.AddForOwnerAsync(request.ToViewModel(), userId);
            return Ok(created);
        }

        /// <summary>
        /// Elimina un inmueble publicado por el propietario.
        /// </summary>
        [HttpDelete("properties/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var property = await _propertyService.GetByIdViewModel(id);
            if (property == null || property.AgentId != userId)
            {
                return NotFound(new { hasError = true, error = "Propiedad no encontrada o no autorizada." });
            }

            await _propertyService.Delete(id);
            return NoContent();
        }

        /// <summary>
        /// Obtiene un inmueble publicado por el propietario para su edición.
        /// </summary>
        [HttpGet("properties/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProperty(int id)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var property = await _propertyService.GetByIdViewModel(id);
            if (property == null || property.AgentId != userId)
            {
                return NotFound(new { hasError = true, error = "Propiedad no encontrada o no autorizada." });
            }

            var propertyDto = _mapper.Map<PropertyDto>(property);
            return Ok(propertyDto);
        }

        /// <summary>
        /// Actualiza un inmueble publicado por el propietario directo.
        /// </summary>
        [HttpPut("properties/{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProperty(int id, [FromForm] SavePropertyFormRequest request)
        {
            var userId = CurrentUserId;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (id != request.Id)
            {
                return BadRequest(new { hasError = true, error = "El ID de la ruta no coincide con el modelo." });
            }

            var property = await _propertyService.GetByIdViewModel(id);
            if (property == null || property.AgentId != userId)
            {
                return NotFound(new { hasError = true, error = "Propiedad no encontrada o no autorizada." });
            }

            var vm = request.ToViewModel();
            vm.AgentId = userId;
            await _propertyService.Update(vm, id);
            return Ok(new { success = true, message = "Inmueble directo actualizado exitosamente." });
        }
    }
}
