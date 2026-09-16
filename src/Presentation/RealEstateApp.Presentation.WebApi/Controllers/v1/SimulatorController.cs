using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class SimulatorController : BaseApiController
    {
        private readonly IFinancingService _financingService;

        public SimulatorController(IFinancingService financingService)
        {
            _financingService = financingService;
        }

        public class MortgageRequest
        {
            public decimal PropertyPrice { get; set; }
            public decimal DownPayment { get; set; }
            public decimal AnnualRate { get; set; } = 11.5m;
            public int TermInYears { get; set; } = 20;
        }

        /// <summary>
        /// Calcula la cuota mensual hipotecaria y la tabla de amortización francesa en RD$.
        /// Endpoint público.
        /// </summary>
        [HttpPost("calculate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MortgageSimulationResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Calculate([FromBody] MortgageRequest request)
        {
            if (request.PropertyPrice <= 0)
            {
                return BadRequest(new { hasError = true, error = "El precio de la propiedad debe ser mayor a 0." });
            }

            if (request.DownPayment < 0 || request.DownPayment >= request.PropertyPrice)
            {
                return BadRequest(new { hasError = true, error = "El pago inicial debe ser menor al precio total del inmueble." });
            }

            if (request.AnnualRate <= 0 || request.AnnualRate > 100)
            {
                return BadRequest(new { hasError = true, error = "La tasa de interés anual debe estar entre 0.1% y 100%." });
            }

            if (request.TermInYears < 1 || request.TermInYears > 40)
            {
                return BadRequest(new { hasError = true, error = "El plazo en años debe estar entre 1 y 40 años." });
            }

            var result = _financingService.CalculateMortgage(request.PropertyPrice, request.DownPayment, request.AnnualRate, request.TermInYears);
            return Ok(result);
        }
    }
}
