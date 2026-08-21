using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Application.DTOs.Currency;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private const string CacheKey = "ExchangeRate_USD_DOP_Info";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
        private const decimal DefaultRate = CurrencyConstants.DefaultUsdToDopRate;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CurrencyService> _logger;

        private static readonly string[] ApiEndpoints = new[]
        {
            "https://open.er-api.com/v6/latest/USD",
            "https://api.exchangerate-api.com/v4/latest/USD"
        };

        public CurrencyService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            ILogger<CurrencyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<decimal> GetExchangeRateAsync()
        {
            var info = await GetExchangeRateInfoAsync();
            return info.Rate;
        }

        public async Task<ExchangeRateInfoDto> GetExchangeRateInfoAsync()
        {
            if (_memoryCache.TryGetValue(CacheKey, out ExchangeRateInfoDto? cachedInfo) && cachedInfo != null)
            {
                return cachedInfo;
            }

            var fetchedInfo = await FetchLiveExchangeRateAsync();

            _memoryCache.Set(CacheKey, fetchedInfo, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.High
            });

            return fetchedInfo;
        }

        private async Task<ExchangeRateInfoDto> FetchLiveExchangeRateAsync()
        {
            var client = _httpClientFactory.CreateClient("ExchangeRateClient");

            foreach (var endpoint in ApiEndpoints)
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(4));
                    var response = await client.GetAsync(endpoint, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync(cts.Token);
                        using var document = JsonDocument.Parse(jsonContent);
                        var root = document.RootElement;

                        if (root.TryGetProperty("rates", out var ratesElement) &&
                            ratesElement.TryGetProperty("DOP", out var dopElement))
                        {
                            var rateValue = dopElement.GetDecimal();
                            if (rateValue > 0)
                            {
                                var roundedRate = Math.Round(rateValue, 2);
                                _logger.LogInformation("Tasa de cambio USD/DOP obtenida en vivo desde {Endpoint}: {Rate}", endpoint, roundedRate);

                                return new ExchangeRateInfoDto
                                {
                                    Rate = roundedRate,
                                    BaseCurrency = CurrencyConstants.USD,
                                    TargetCurrency = CurrencyConstants.DOP,
                                    LastUpdated = DateTime.UtcNow,
                                    IsLive = true,
                                    Provider = "ExchangeRate-API (En Línea)"
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al consultar la API de cotización en línea ({Endpoint}). Intentando siguiente opción...", endpoint);
                }
            }

            // Contingencia / Fallback si todas las llamadas fallan
            _logger.LogWarning("No se pudo obtener la tasa de cambio en vivo desde los proveedores en línea. Usando tasa de contingencia ({DefaultRate})", DefaultRate);

            return new ExchangeRateInfoDto
            {
                Rate = DefaultRate,
                BaseCurrency = CurrencyConstants.USD,
                TargetCurrency = CurrencyConstants.DOP,
                LastUpdated = DateTime.UtcNow,
                IsLive = false,
                Provider = "Tasa de Referencia (Contingencia)"
            };
        }

        public decimal ConvertToDOP(decimal amount, string fromCurrency, decimal exchangeRate)
        {
            if (string.Equals(fromCurrency, CurrencyConstants.USD, StringComparison.OrdinalIgnoreCase))
            {
                var rate = exchangeRate > 0 ? exchangeRate : DefaultRate;
                return Math.Round(amount * rate, 2);
            }
            return amount;
        }

        public decimal ConvertToUSD(decimal amount, string fromCurrency, decimal exchangeRate)
        {
            if (string.Equals(fromCurrency, CurrencyConstants.DOP, StringComparison.OrdinalIgnoreCase))
            {
                var rate = exchangeRate > 0 ? exchangeRate : DefaultRate;
                return rate > 0 ? Math.Round(amount / rate, 2) : amount;
            }
            return amount;
        }

        public decimal Convert(decimal amount, string fromCurrency, string targetCurrency, decimal exchangeRate)
        {
            if (string.Equals(fromCurrency, targetCurrency, StringComparison.OrdinalIgnoreCase))
            {
                return amount;
            }

            if (string.Equals(targetCurrency, CurrencyConstants.USD, StringComparison.OrdinalIgnoreCase))
            {
                return ConvertToUSD(amount, fromCurrency, exchangeRate);
            }

            return ConvertToDOP(amount, fromCurrency, exchangeRate);
        }

        public string FormatPrice(decimal amount, string currency)
        {
            var isUSD = string.Equals(currency, CurrencyConstants.USD, StringComparison.OrdinalIgnoreCase);
            var prefix = isUSD ? "US$ " : "RD$ ";
            return $"{prefix}{amount.ToString("N0", CultureInfo.InvariantCulture)}";
        }

        public string FormatSecondaryPrice(decimal amount, string originalCurrency, string activeCurrency, decimal exchangeRate)
        {
            var targetSecondary = string.Equals(activeCurrency, CurrencyConstants.USD, StringComparison.OrdinalIgnoreCase) 
                ? CurrencyConstants.DOP 
                : CurrencyConstants.USD;

            var converted = Convert(amount, activeCurrency, targetSecondary, exchangeRate);
            var prefix = string.Equals(targetSecondary, CurrencyConstants.USD, StringComparison.OrdinalIgnoreCase) ? "US$ " : "RD$ ";
            return $"~{prefix}{converted.ToString("N0", CultureInfo.InvariantCulture)}";
        }
    }
}
