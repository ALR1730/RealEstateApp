using System.Collections.Generic;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IFinancingService
    {
        // Realiza simulación de cuotas usando el sistema de amortización francés
        List<AmortizationScheduleItem> GenerateAmortizationSchedule(decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears);
        MortgageSimulationResult CalculateMortgage(decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears);
    }

    public class AmortizationScheduleItem
    {
        public int Period { get; set; }
        public decimal Installment { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal RemainingBalance { get; set; }
    }

    public class MortgageSimulationResult
    {
        public decimal PropertyPrice { get; set; }
        public decimal DownPayment { get; set; }
        public decimal DownPaymentPercentage { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal AnnualRate { get; set; }
        public int TermInYears { get; set; }
        public int TotalMonths { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalCost { get; set; }
        public List<AmortizationScheduleItem> Schedule { get; set; } = new();
    }
}
