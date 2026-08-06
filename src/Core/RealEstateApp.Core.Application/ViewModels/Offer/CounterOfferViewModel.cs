using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Offer
{
    /// <summary>
    /// ViewModel para la acción de realizar una contra-oferta por parte del Agente.
    /// </summary>
    public class CounterOfferViewModel
    {
        [Required(ErrorMessage = "La oferta original es requerida.")]
        public int OfferId { get; set; }

        [Required(ErrorMessage = "El monto de la contra-oferta es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        [Display(Name = "Monto de Contra-Oferta (RD$)")]
        public decimal CounterOfferAmount { get; set; }

        [StringLength(500, ErrorMessage = "La nota o mensaje no puede superar los 500 caracteres.")]
        [Display(Name = "Nota o Términos del Agente")]
        public string? CounterOfferMessage { get; set; }
    }
}
