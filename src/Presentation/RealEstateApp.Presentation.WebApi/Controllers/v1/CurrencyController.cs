using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class CurrencyController : BaseApiController
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// Obtiene la tasa de cambio actual USD / DOP y cotizaciones de divisas.
        /// </summary>
        [HttpGet("rates")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRates()
        {
            var rate = await _currencyService.GetExchangeRateAsync();
            return Ok(new
            {
                baseCurrency = "USD",
                targetCurrency = "DOP",
                exchangeRate = rate,
                formattedRate = $"1 USD = {rate:N2} RD$"
            });
        }
    }
}
