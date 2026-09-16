using System;

namespace RealEstateApp.Core.Application.ViewModels.BuyAbility
{
    public class BuyAbilityRequestDto
    {
        public decimal MonthlyGrossIncome { get; set; }
        public decimal MonthlyNetIncome { get; set; }
        public decimal MonthlyDebtPayments { get; set; }
        public decimal AvailableDownPayment { get; set; }
        public string? Currency { get; set; }
    }

    public class BuyAbilityResultDto
    {
        public int? EvaluationId { get; set; }
        public string ClientId { get; set; } = string.Empty;

        public decimal MonthlyGrossIncome { get; set; }
        public decimal MonthlyNetIncome { get; set; }
        public decimal MonthlyDebtPayments { get; set; }
        public decimal AvailableDownPayment { get; set; }

        public decimal MaxMonthlyPayment { get; set; }
        public decimal MaxMortgageAmount { get; set; }
        public decimal MaxPropertyPrice { get; set; }

        public decimal DebtToIncomeRatio { get; set; }
        public string CreditScoreRating { get; set; } = string.Empty;
        public decimal EstimatedAnnualRate { get; set; }
        public int RecommendedTermYears { get; set; }

        public string EvaluationResult { get; set; } = string.Empty; // Aprobado, Pre-Aprobado, No Aprobado
        public string Currency { get; set; } = "DOP";
        public string? Observations { get; set; }
        public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;
    }
}
