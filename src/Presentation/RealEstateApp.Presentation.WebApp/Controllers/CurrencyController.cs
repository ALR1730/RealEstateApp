using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class CurrencyController : Controller
    {
        public const string CurrencyCookieName = "_selectedCurrency";
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpPost]
        [HttpGet]
        public IActionResult SetCurrency(string currency, string? returnUrl = null)
        {
            var selectedCurrency = string.Equals(currency, "USD", StringComparison.OrdinalIgnoreCase) ? "USD" : "DOP";

            // Guardar en Cookie por 1 año
            Response.Cookies.Append(
                CurrencyCookieName,
                selectedCurrency,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    HttpOnly = false
                }
            );

            // Guardar también en Sesión si está disponible
            try
            {
                HttpContext.Session?.SetString("SelectedCurrency", selectedCurrency);
            }
            catch
            {
                // Si la sesión no está disponible en este request, el valor persiste en la cookie
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> GetLiveRate()
        {
            var rateInfo = await _currencyService.GetExchangeRateInfoAsync();
            return Json(new
            {
                rate = rateInfo.Rate,
                baseCurrency = rateInfo.BaseCurrency,
                targetCurrency = rateInfo.TargetCurrency,
                lastUpdated = rateInfo.LastUpdated.ToString("g"),
                isLive = rateInfo.IsLive,
                provider = rateInfo.Provider
            });
        }
    }
}
