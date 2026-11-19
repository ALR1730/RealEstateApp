using System;
using System.Collections.Generic;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Domain.Services
{
    public sealed record AmortizationInstallment(
        int Period,
        decimal Installment,
        decimal Interest,
        decimal Principal,
        decimal RemainingBalance);

    public sealed record MortgageCalculation(
        decimal PropertyPrice,
        decimal DownPayment,
        decimal DownPaymentPercentage,
        decimal LoanAmount,
        decimal AnnualRate,
        int TermInYears,
        int TotalMonths,
        decimal MonthlyInstallment,
        decimal TotalInterest,
        decimal TotalCost,
        IReadOnlyList<AmortizationInstallment> Schedule);

    /// <summary>
    /// Calculadora de dominio pura para amortización hipotecaria bajo el sistema francés.
    /// </summary>
    public static class MortgageAmortizationCalculator
    {
        public const int MinTermYears = 1;
        public const int MaxTermYears = 40;
        public const decimal MinAnnualRate = 0m;
        public const decimal MaxAnnualRate = 100m;

        public static IReadOnlyList<AmortizationInstallment> GenerateSchedule(
            decimal propertyPrice,
            decimal downPayment,
            decimal annualRate,
            int termInYears)
        {
            ValidateParameters(propertyPrice, downPayment, annualRate, termInYears);

            var schedule = new List<AmortizationInstallment>();
            decimal loanAmount = propertyPrice - downPayment;
            if (loanAmount <= 0)
            {
                return schedule;
            }

            decimal monthlyRate = annualRate / 12m / 100m;
            int totalPeriods = termInYears * 12;

            decimal monthlyInstallment = CalculateBaseInstallment(loanAmount, monthlyRate, totalPeriods);
            monthlyInstallment = Math.Round(monthlyInstallment, 2);

            decimal remainingBalance = loanAmount;

            for (int period = 1; period <= totalPeriods; period++)
            {
                decimal interestPayment = Math.Round(remainingBalance * monthlyRate, 2);
                decimal principalPayment = monthlyInstallment - interestPayment;

                if (period == totalPeriods)
                {
                    principalPayment = remainingBalance;
                    monthlyInstallment = principalPayment + interestPayment;
                }

                remainingBalance -= principalPayment;
                if (remainingBalance < 0)
                {
                    remainingBalance = 0;
                }

                schedule.Add(new AmortizationInstallment(
                    Period: period,
                    Installment: Math.Round(monthlyInstallment, 2),
                    Interest: interestPayment,
                    Principal: Math.Round(principalPayment, 2),
                    RemainingBalance: Math.Round(remainingBalance, 2)));
            }

            return schedule;
        }

        public static MortgageCalculation Calculate(
            decimal propertyPrice,
            decimal downPayment,
            decimal annualRate,
            int termInYears)
        {
            ValidateParameters(propertyPrice, downPayment, annualRate, termInYears);

            var schedule = GenerateSchedule(propertyPrice, downPayment, annualRate, termInYears);
            decimal loanAmount = propertyPrice > downPayment ? propertyPrice - downPayment : 0m;
            decimal totalInterest = 0m;
            decimal monthlyInstallment = 0m;

            if (schedule.Count > 0)
            {
                monthlyInstallment = schedule[0].Installment;
                foreach (var item in schedule)
                {
                    totalInterest += item.Interest;
                }
            }

            decimal downPaymentPercentage = propertyPrice > 0
                ? Math.Round((downPayment / propertyPrice) * 100m, 2)
                : 0m;

            return new MortgageCalculation(
                PropertyPrice: Math.Round(propertyPrice, 2),
                DownPayment: Math.Round(downPayment, 2),
                DownPaymentPercentage: downPaymentPercentage,
                LoanAmount: Math.Round(loanAmount, 2),
                AnnualRate: Math.Round(annualRate, 2),
                TermInYears: termInYears,
                TotalMonths: termInYears * 12,
                MonthlyInstallment: Math.Round(monthlyInstallment, 2),
                TotalInterest: Math.Round(totalInterest, 2),
                TotalCost: Math.Round(loanAmount + totalInterest, 2),
                Schedule: schedule);
        }

        private static decimal CalculateBaseInstallment(decimal loanAmount, decimal monthlyRate, int totalPeriods)
        {
            if (monthlyRate == 0)
            {
                return loanAmount / totalPeriods;
            }

            double rateDouble = (double)monthlyRate;
            double factor = Math.Pow(1 + rateDouble, totalPeriods);
            return loanAmount * (decimal)(rateDouble * factor / (factor - 1));
        }

        private static void ValidateParameters(
            decimal propertyPrice,
            decimal downPayment,
            decimal annualRate,
            int termInYears)
        {
            if (propertyPrice <= 0)
            {
                throw new ValidationException("El precio de la propiedad debe ser mayor a cero.");
            }

            if (downPayment < 0 || downPayment >= propertyPrice)
            {
                throw new ValidationException("El pago inicial debe ser mayor o igual a cero y menor al precio de la propiedad.");
            }

            if (annualRate < MinAnnualRate || annualRate > MaxAnnualRate)
            {
                throw new ValidationException("La tasa de interés anual debe estar entre 0% y 100%.");
            }

            if (termInYears < MinTermYears || termInYears > MaxTermYears)
            {
                throw new ValidationException("El plazo en años debe estar entre 1 y 40 años.");
            }
        }
    }
}
