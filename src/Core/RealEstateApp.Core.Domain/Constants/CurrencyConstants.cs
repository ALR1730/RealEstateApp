namespace RealEstateApp.Core.Domain.Constants
{
    public static class CurrencyConstants
    {
        public const string DOP = "DOP";
        public const string USD = "USD";

        public const string DefaultCurrency = DOP;

        // Tasa de cambio de referencia estándar
        public const decimal DefaultUsdToDopRate = 60.50m;
    }
}
