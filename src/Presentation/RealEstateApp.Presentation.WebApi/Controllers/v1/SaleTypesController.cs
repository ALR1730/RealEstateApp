using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.SaleType;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.SaleType;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class SaleTypesController : BaseApiController
    {
        private readonly ISaleTypeService _saleTypeService;
        private readonly IMapper _mapper;

        public SaleTypesController(ISaleTypeService saleTypeService, IMapper mapper)
        {
            _saleTypeService = saleTypeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtiene el listado de todos los tipos de venta.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SaleTypeDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetAsync()
        {
            var list = await _saleTypeService.GetAllViewModel();

            if (list == null || list.Count == 0)
            {
                return Ok(new List<SaleTypeDto>());
            }

            var dtos = _mapper.Map<List<SaleTypeDto>>(list);
            return Ok(dtos);
        }

        /// <summary>
        /// Obtiene un tipo de venta por su ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaleTypeDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var saveVm = await _saleTypeService.GetByIdSaveViewModel(id);

            if (saveVm == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún tipo de venta con el ID {id}" });
            }

            var dto = _mapper.Map<SaleTypeDto>(saveVm);
            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo tipo de venta (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SaleTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PostAsync([FromBody] SaveSaleTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdVm = await _saleTypeService.Add(vm);
            var dto = _mapper.Map<SaleTypeDto>(createdVm);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
        }

        /// <summary>
        /// Actualiza un tipo de venta existente (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaleTypeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] SaveSaleTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existing = await _saleTypeService.GetByIdSaveViewModel(id);
            if (existing == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún tipo de venta con el ID {id}" });
            }

            vm.Id = id;
            await _saleTypeService.Update(vm, id);
            var updatedVm = await _saleTypeService.GetByIdSaveViewModel(id);
            var dto = _mapper.Map<SaleTypeDto>(updatedVm);

            return Ok(dto);
        }

        /// <summary>
        /// Elimina un tipo de venta (Reservado para Administradores y Desarrolladores).
        /// </summary>
        [Authorize(Roles = "Admin,Developer")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var existing = await _saleTypeService.GetByIdSaveViewModel(id);
            if (existing == null)
            {
                return NotFound(new { hasError = true, error = $"No se encontró ningún tipo de venta con el ID {id}" });
            }

            await _saleTypeService.Delete(id);
            return NoContent();
        }
    }
}
