using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Valuation;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class ValuationController : BaseApiController
    {
        private readonly IPropertyValuationService _valuationService;

        public ValuationController(IPropertyValuationService valuationService)
        {
            _valuationService = valuationService;
        }

        /// <summary>
        /// Calcula la valuación automatizada (AVM) de una propiedad basándose en comparables.
        /// </summary>
        [Authorize(Roles = "Agent,Admin")]
        [HttpGet("properties/{propertyId:int}/valuation")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ValuationResultDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CalculateValuation(int propertyId, [FromQuery] decimal searchRadiusKm = 5.0m)
        {
            try
            {
                var result = await _valuationService.CalculateValuationAsync(propertyId, searchRadiusKm);
                return Ok(result);
            }
            catch (RealEstateApp.Core.Domain.Exceptions.NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la última valuación almacenada de una propiedad.
        /// </summary>
        [Authorize(Roles = "Agent,Admin")]
        [HttpGet("properties/{propertyId:int}/valuation/last")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ValuationResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetLastValuation(int propertyId)
        {
            var result = await _valuationService.GetLastValuationAsync(propertyId);
            if (result == null)
            {
                return NoContent();
            }
            return Ok(result);
        }
    }
}
