using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Improvement;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Improvement;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class ImprovementsController : BaseApiController
    {
        private readonly IImprovementService _improvementService;
        private readonly IMapper _mapper;

        public ImprovementsController(IImprovementService improvementService, IMapper mapper)
        {
            _improvementService = improvementService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene el listado de todas las mejoras de propiedades.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ImprovementDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAsync()
        {
            var list = await _improvementService.GetAllViewModel();

            if (list == null || list.Count == 0)
            {
                return NoContent();
            }

            var dtos = _mapper.Map<List<ImprovementDto>>(list);
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene una mejora por su ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImprovementDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var saveVm = await _improvementService.GetByIdSaveViewModel(id);

            if (saveVm == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ninguna mejora con el ID {id}" });
            }

            var dto = _mapper.Map<ImprovementDto>(saveVm);
            return Ok(dto);
        }

        /// <summary>
        /// Crea una nueva mejora (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ImprovementDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostAsync([FromBody] SaveImprovementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdVm = await _improvementService.Add(vm);
            var dto = _mapper.Map<ImprovementDto>(createdVm);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
        }

        /// <summary>
        /// Actualiza una mejora existente (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ImprovementDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] SaveImprovementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _improvementService.GetByIdSaveViewModel(id);
            if (existing == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ninguna mejora con el ID {id}" });
            }

            vm.Id = id;
            await _improvementService.Update(vm, id);
            var updatedVm = await _improvementService.GetByIdSaveViewModel(id);
            var dto = _mapper.Map<ImprovementDto>(updatedVm);

            return Ok(dto);
        }

        /// <summary>
        /// Elimina una mejora (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var existing = await _improvementService.GetByIdSaveViewModel(id);
            if (existing == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ninguna mejora con el ID {id}" });
            }

            await _improvementService.Delete(id);
            return NoContent();
        }
    }
}
