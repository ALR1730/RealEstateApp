using System;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Entidad para el sistema BuyAbility - Evaluación de Capacidad de Compra.
    /// Analiza ingresos, deudas y perfil financiero del cliente para determinar su poder adquisitivo.
    /// </summary>
    public class BuyAbilityEvaluation : AuditableBaseEntity
    {
        /// <summary>
        /// ID del usuario cliente evaluado.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Ingreso mensual bruto del cliente.
        /// </summary>
        public decimal MonthlyGrossIncome { get; set; }

        /// <summary>
        /// Ingreso mensual neto del cliente (después de impuestos).
        /// </summary>
        public decimal MonthlyNetIncome { get; set; }

        /// <summary>
        /// Total de deudas mensuales actuales (cuotas de préstamos, tarjetas, etc.).
        /// </summary>
        public decimal MonthlyDebtPayments { get; set; }

        /// <summary>
        /// Ahorro acumulado disponible para pago inicial.
        /// </summary>
        public decimal AvailableDownPayment { get; set; }

        /// <summary>
        /// Monto máximo de préstamo hipotecario aprobado.
        /// </summary>
        public decimal MaxMortgageAmount { get; set; }

        /// <summary>
        /// Precio máximo de propiedad que el cliente puede costear.
        /// </summary>
        public decimal MaxPropertyPrice { get; set; }

        /// <summary>
        /// Cuota mensual máxima estimada (basada en regla del 30% del ingreso neto).
        /// </summary>
        public decimal MaxMonthlyPayment { get; set; }

        /// <summary>
        /// Ratio deuda-ingreso (Debt-to-Income ratio, porcentaje).
        /// </summary>
        public decimal DebtToIncomeRatio { get; set; }

        /// <summary>
        /// Calificación crediticia estimada (Excelente, Bueno, Regular, Bajo).
        /// </summary>
        public string CreditScoreRating { get; set; } = "No Evaluado";

        /// <summary>
        /// Tasa de interés anual estimada según el perfil.
        /// </summary>
        public decimal EstimatedAnnualRate { get; set; }

        /// <summary>
        /// Plazo máximo recomendado en años.
        /// </summary>
        public int RecommendedTermYears { get; set; } = 20;

        /// <summary>
        /// Moneda de la evaluación.
        /// </summary>
        public string Currency { get; set; } = "DOP";

        /// <summary>
        /// Resultado de la evaluación: Aprobado, Pre-Aprobado, No Aprobado.
        /// </summary>
        public string EvaluationResult { get; set; } = "Pendiente";

        /// <summary>
        /// Fecha de la evaluación.
        /// </summary>
        public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Observaciones o recomendaciones adicionales.
        /// </summary>
        public string? Observations { get; set; }

        /// <summary>
        /// Indica si la evaluación está activa (no expirada).
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
