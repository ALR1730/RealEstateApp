using System;

namespace RealEstateApp.Core.Application.DTOs.Currency
{
    public class ExchangeRateInfoDto
    {
        public decimal Rate { get; set; }
        public string BaseCurrency { get; set; } = "USD";
        public string TargetCurrency { get; set; } = "DOP";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsLive { get; set; } = true;
        public string Provider { get; set; } = "ExchangeRate-API (En línea)";
    }
}
