using System;
using System.Globalization;

namespace RealEstateApp.Core.Application.ViewModels.Property
{
    /// <summary>
    /// ViewModel para la visualización del historial y evolución de precios de un inmueble.
    /// </summary>
    public class PriceHistoryViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string Currency { get; set; } = "DOP";
        public decimal PercentageChange { get; set; }
        public DateTime ChangeDate { get; set; }
        public string? ChangedByUserId { get; set; }
        public string? ChangeReason { get; set; }

        public bool IsPriceDrop => PercentageChange < 0;
        public bool IsInitialPrice => OldPrice == 0;

        public string FormattedOldPrice => $"{GetCurrencyPrefix()}{OldPrice.ToString("N0", CultureInfo.InvariantCulture)}";
        public string FormattedNewPrice => $"{GetCurrencyPrefix()}{NewPrice.ToString("N0", CultureInfo.InvariantCulture)}";
        public string FormattedDifference => $"{GetCurrencyPrefix()}{Math.Abs(NewPrice - OldPrice).ToString("N0", CultureInfo.InvariantCulture)}";
        public string FormattedDate => ChangeDate.ToString("dd/MM/yyyy HH:mm");
        public string FormattedPercentage => $"{PercentageChange:+#,##0.0;-#,##0.0;0.0}%";

        private string GetCurrencyPrefix() => string.Equals(Currency, "USD", StringComparison.OrdinalIgnoreCase) ? "US$ " : "RD$ ";
    }
}
