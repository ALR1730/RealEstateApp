namespace RealEstateApp.Core.Domain.Constants
{
    /// <summary>
    /// Constantes centralizadas para los estados de una comisión.
    /// </summary>
    public static class CommissionStatus
    {
        public const string Pending = "Pendiente";
        public const string Paid = "Pagada";
    }

    /// <summary>
    /// Constante global para la tasa de comisión por defecto (5%).
    /// Se usa cuando el plan de suscripción del agente no define un porcentaje propio.
    /// </summary>
    public static class CommissionConstants
    {
        public const decimal DefaultRate = 5.00m;
    }
}
