using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.BuyAbility;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Client")]
    public class BuyAbilityController : BaseApiController
    {
        private readonly IBuyAbilityService _buyAbilityService;

        public BuyAbilityController(IBuyAbilityService buyAbilityService)
        {
            _buyAbilityService = buyAbilityService;
        }

        /// <summary>
        /// Evalúa la capacidad de compra del cliente basándose en su perfil financiero.
        /// </summary>
        [HttpPost("evaluate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BuyAbilityResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Evaluate([FromBody] BuyAbilityRequestDto request)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _buyAbilityService.EvaluateAsync(clientId, request);
                return Ok(result);
            }
            catch (RealEstateApp.Core.Domain.Exceptions.ValidationException ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la última evaluación de capacidad de compra del cliente.
        /// </summary>
        [HttpGet("last")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BuyAbilityResultDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetLastEvaluation()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var result = await _buyAbilityService.GetLastEvaluationAsync(clientId);
            if (result == null)
            {
                return NoContent();
            }
            return Ok(result);
        }
    }
}
