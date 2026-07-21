using System;
using System.Collections.Generic;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    /// <summary>
    /// Implementación del simulador hipotecario usando el sistema de amortización francesa.
    /// Cuota fija mensual donde cada periodo se descompone en interés y amortización de capital.
    /// </summary>
    public class FinancingService : IFinancingService
    {
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
    }
}
