using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Owner")]
    public class OwnersController : BaseApiController
    {
        private const int MaxOwnerProperties = 2;

        private readonly IPropertyService _propertyService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public OwnersController(
            IPropertyService propertyService,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _propertyService = propertyService;
            _userManager = userManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene los inmuebles publicados por el propietario directo (máximo 2).
        /// </summary>
        [HttpGet("my-properties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProperties()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var properties = await _propertyService.GetByAgentId(userId);
            return Ok(new
            {
                maxAllowed = MaxOwnerProperties,
                currentCount = properties.Count,
                canCreate = properties.Count < MaxOwnerProperties,
                properties
            });
        }

        /// <summary>
        /// Publica un inmueble directo como propietario (hasta 2 máximo).
        /// </summary>
        [HttpPost("properties")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProperty([FromForm] SavePropertyViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var properties = await _propertyService.GetByAgentId(userId);
            if (properties.Count >= MaxOwnerProperties)
            {
                return BadRequest(new { hasError = true, error = $"Como propietario directo puedes publicar un máximo de {MaxOwnerProperties} inmuebles simultáneos." });
            }

            vm.AgentId = userId;
            var created = await _propertyService.Add(vm);
            if (created == null)
            {
                return BadRequest(new { hasError = true, error = "Error al registrar la propiedad." });
            }

            return Ok(created);
        }

        /// <summary>
        /// Elimina un inmueble publicado por el propietario.
        /// </summary>
        [HttpDelete("properties/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var userId = _userManager.GetUserId(User);
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
            var userId = _userManager.GetUserId(User);
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
        public async Task<IActionResult> UpdateProperty(int id, [FromForm] SavePropertyViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (id != vm.Id)
            {
                return BadRequest(new { hasError = true, error = "El ID de la ruta no coincide con el modelo." });
            }

            var property = await _propertyService.GetByIdViewModel(id);
            if (property == null || property.AgentId != userId)
            {
                return NotFound(new { hasError = true, error = "Propiedad no encontrada o no autorizada." });
            }

            vm.AgentId = userId;
            await _propertyService.Update(vm, id);
            return Ok(new { success = true, message = "Inmueble directo actualizado exitosamente." });
        }
    }
}
