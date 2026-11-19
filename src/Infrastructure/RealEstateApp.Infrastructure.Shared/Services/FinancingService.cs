using System.Collections.Generic;
using System.Linq;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Services;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    /// <summary>
    /// Adaptador de infraestructura compartida que delega el cálculo hipotecario en el dominio puro.
    /// </summary>
    public class FinancingService : IFinancingService
    {
        public List<AmortizationScheduleItem> GenerateAmortizationSchedule(
            decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears)
        {
            var domainSchedule = MortgageAmortizationCalculator.GenerateSchedule(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            return MapSchedule(domainSchedule);
        }

        public MortgageSimulationResult CalculateMortgage(
            decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears)
        {
            var calculation = MortgageAmortizationCalculator.Calculate(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            return new MortgageSimulationResult
            {
                PropertyPrice = calculation.PropertyPrice,
                DownPayment = calculation.DownPayment,
                DownPaymentPercentage = calculation.DownPaymentPercentage,
                LoanAmount = calculation.LoanAmount,
                AnnualRate = calculation.AnnualRate,
                TermInYears = calculation.TermInYears,
                TotalMonths = calculation.TotalMonths,
                MonthlyInstallment = calculation.MonthlyInstallment,
                TotalInterest = calculation.TotalInterest,
                TotalCost = calculation.TotalCost,
                Schedule = MapSchedule(calculation.Schedule)
            };
        }

        private static List<AmortizationScheduleItem> MapSchedule(IReadOnlyList<AmortizationInstallment> schedule)
        {
            return schedule.Select(item => new AmortizationScheduleItem
            {
                Period = item.Period,
                Installment = item.Installment,
                Interest = item.Interest,
                Principal = item.Principal,
                RemainingBalance = item.RemainingBalance
            }).ToList();
        }
    }
}
