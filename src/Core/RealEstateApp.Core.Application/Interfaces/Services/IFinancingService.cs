using System.Collections.Generic;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IFinancingService
    {
        // Realiza simulación de cuotas usando el sistema de amortización francés
        List<AmortizationScheduleItem> GenerateAmortizationSchedule(decimal propertyPrice, decimal downPayment, decimal annualRate, int termInYears);
    }

    public class AmortizationScheduleItem
    {
        public int Period { get; set; }
        public decimal Installment { get; set; }
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
