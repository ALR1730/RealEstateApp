using System.Collections.Generic;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Core.Application.ViewModels.MortgageSimulation
{
    /// <summary>
    /// ViewModel para mostrar los resultados de la simulación hipotecaria.
    /// Contiene la tabla de amortización generada y un resumen del cálculo.
    /// </summary>
    public class MortgageSimulationResultViewModel
    {
        /// <summary>
        /// Monto total del préstamo (precio - enganche).
        /// </summary>
        public decimal LoanAmount { get; set; }

        /// <summary>
        /// Cuota mensual fija calculada.
        /// </summary>
        public decimal MonthlyInstallment { get; set; }

        /// <summary>
        /// Total de intereses pagados durante todo el plazo.
        /// </summary>
        public decimal TotalInterest { get; set; }

        /// <summary>
        /// Monto total pagado (capital + intereses).
        /// </summary>
        public decimal TotalPaid { get; set; }

        /// <summary>
        /// Plazo del crédito en años.
        /// </summary>
        public int TermInYears { get; set; }

        /// <summary>
        /// Tasa de interés anual aplicada.
        /// </summary>
        public decimal AnnualRate { get; set; }

        /// <summary>
        /// Precio original de la propiedad.
        /// </summary>
        public decimal PropertyPrice { get; set; }

        /// <summary>
        /// Enganche/inicial aportado.
        /// </summary>
        public decimal DownPayment { get; set; }

        /// <summary>
        /// Tabla de amortización completa mes a mes.
        /// </summary>
        public List<AmortizationScheduleItem> Schedule { get; set; } = new();
    }
}
