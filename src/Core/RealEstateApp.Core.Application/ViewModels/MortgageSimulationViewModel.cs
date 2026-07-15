using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels
{
    public class MortgageSimulationViewModel
    {
        [Required(ErrorMessage = "El precio de la propiedad es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
        public decimal PropertyPrice { get; set; }

        [Required(ErrorMessage = "El monto inicial aportado es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto inicial no puede ser negativo")]
        public decimal MontoInicialAportado { get; set; }

        [Required(ErrorMessage = "La tasa de interés anual es requerida")]
        [Range(0.1, 100, ErrorMessage = "La tasa debe estar entre 0.1 y 100")]
        public decimal TasaInteresAnual { get; set; }

        [Required(ErrorMessage = "El plazo en años es requerido")]
        [Range(1, 40, ErrorMessage = "El plazo debe estar entre 1 y 40 años")]
        public int TermInYears { get; set; }
    }
}
