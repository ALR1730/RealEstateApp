using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Currency;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetExchangeRateAsync();
        Task<ExchangeRateInfoDto> GetExchangeRateInfoAsync();
        decimal ConvertToDOP(decimal amount, string fromCurrency, decimal exchangeRate);
        decimal ConvertToUSD(decimal amount, string fromCurrency, decimal exchangeRate);
        decimal Convert(decimal amount, string fromCurrency, string targetCurrency, decimal exchangeRate);
        string FormatPrice(decimal amount, string currency);
        string FormatSecondaryPrice(decimal amount, string originalCurrency, string activeCurrency, decimal exchangeRate);
    }
}
