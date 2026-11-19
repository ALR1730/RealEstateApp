using System;
using System.Collections.Generic;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    /// <summary>
    /// Implementación del simulador hipotecario usando el sistema de amortización francesa.
    /// Cuota fija mensual donde cada periodo se descompone en interés y amortización de capital.
    /// </summary>
    public class FinancingService : IFinancingService
    {
        private const int MinTermYears = 1;
        private const int MaxTermYears = 40;
        private const decimal MinAnnualRate = 0m;
        private const decimal MaxAnnualRate = 100m;

        /// <summary>
        /// Genera la tabla de amortización completa usando el sistema francés.
        /// Fórmula: M = P × [r(1+r)^n] / [(1+r)^n − 1]
        /// </summary>
        /// <param name="propertyPrice">Precio total del inmueble.</param>
        /// <param name="downPayment">Monto del enganche/inicial aportado.</param>
        /// <param name="annualRate">Tasa de interés anual (ej: 8.5 para 8.5%).</param>
        /// <param name="termInYears">Plazo del crédito en años.</param>
        /// <returns>Lista de cuotas con desglose de interés, capital y saldo restante.</returns>
        public List<AmortizationScheduleItem> GenerateAmortizationSchedule(
            decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears)
        {
            ValidateParameters(propertyPrice, downPayment, annualRate, termInYears);

            var schedule = new List<AmortizationScheduleItem>();

            // Monto del préstamo = Precio - Enganche
            decimal loanAmount = propertyPrice - downPayment;

            if (loanAmount <= 0)
                return schedule;

            // Tasa de interés mensual
            decimal monthlyRate = annualRate / 12m / 100m;

            // Total de periodos (meses)
            int totalPeriods = termInYears * 12;

            // Cálculo de la cuota fija mensual (amortización francesa)
            // M = P × [r(1+r)^n] / [(1+r)^n − 1]
            decimal monthlyInstallment;

            if (monthlyRate == 0)
            {
                // Caso especial: tasa de interés 0%
                monthlyInstallment = loanAmount / totalPeriods;
            }
            else
            {
                double rateDouble = (double)monthlyRate;
                double factor = Math.Pow(1 + rateDouble, totalPeriods);
                monthlyInstallment = loanAmount * (decimal)(rateDouble * factor / (factor - 1));
            }

            // Redondear la cuota a 2 decimales
            monthlyInstallment = Math.Round(monthlyInstallment, 2);

            decimal remainingBalance = loanAmount;

            for (int period = 1; period <= totalPeriods; period++)
            {
                // Interés del periodo = Saldo restante × Tasa mensual
                decimal interestPayment = Math.Round(remainingBalance * monthlyRate, 2);

                // Capital amortizado = Cuota fija − Interés
                decimal principalPayment = monthlyInstallment - interestPayment;

                // Ajustar el último periodo para evitar errores de redondeo
                if (period == totalPeriods)
                {
                    principalPayment = remainingBalance;
                    monthlyInstallment = principalPayment + interestPayment;
                }

                remainingBalance -= principalPayment;

                // Evitar saldos negativos por redondeo
                if (remainingBalance < 0)
                    remainingBalance = 0;

                schedule.Add(new AmortizationScheduleItem
                {
                    Period = period,
                    Installment = Math.Round(monthlyInstallment, 2),
                    Interest = interestPayment,
                    Principal = Math.Round(principalPayment, 2),
                    RemainingBalance = Math.Round(remainingBalance, 2)
                });
            }

            return schedule;
        }

        public MortgageSimulationResult CalculateMortgage(
            decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears)
        {
            ValidateParameters(propertyPrice, downPayment, annualRate, termInYears);

            var schedule = GenerateAmortizationSchedule(propertyPrice, downPayment, annualRate, termInYears);

            decimal loanAmount = propertyPrice > downPayment ? propertyPrice - downPayment : 0;
            decimal totalInterest = 0;
            decimal monthlyInstallment = 0;

            if (schedule.Count > 0)
            {
                monthlyInstallment = schedule[0].Installment;
                foreach (var item in schedule)
                {
                    totalInterest += item.Interest;
                }
            }

            decimal downPaymentPercentage = propertyPrice > 0 ? Math.Round((downPayment / propertyPrice) * 100m, 2) : 0;

            return new MortgageSimulationResult
            {
                PropertyPrice = Math.Round(propertyPrice, 2),
                DownPayment = Math.Round(downPayment, 2),
                DownPaymentPercentage = downPaymentPercentage,
                LoanAmount = Math.Round(loanAmount, 2),
                AnnualRate = Math.Round(annualRate, 2),
                TermInYears = termInYears,
                TotalMonths = termInYears * 12,
                MonthlyInstallment = Math.Round(monthlyInstallment, 2),
                TotalInterest = Math.Round(totalInterest, 2),
                TotalCost = Math.Round(loanAmount + totalInterest, 2),
                Schedule = schedule
            };
        }

        private static void ValidateParameters(decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears)
        {
            if (propertyPrice <= 0)
                throw new ValidationException("El precio de la propiedad debe ser mayor a cero.");

            if (downPayment < 0 || downPayment >= propertyPrice)
                throw new ValidationException("El pago inicial debe ser mayor o igual a cero y menor al precio de la propiedad.");

            if (annualRate < MinAnnualRate || annualRate > MaxAnnualRate)
                throw new ValidationException("La tasa de interés anual debe estar entre 0% y 100%.");

            if (termInYears < MinTermYears || termInYears > MaxTermYears)
                throw new ValidationException("El plazo en años debe estar entre 1 y 40 años.");
        }
    }
}
