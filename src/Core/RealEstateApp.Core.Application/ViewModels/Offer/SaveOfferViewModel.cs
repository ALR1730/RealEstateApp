using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Core.Application.ViewModels.Offer
{
    /// <summary>
    /// ViewModel para formulario de envío de oferta por parte del cliente.
    /// Incluye carga de carta de pre-aprobación bancaria.
    /// </summary>
    public class SaveOfferViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El monto ofertado es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        [DataType(DataType.Currency)]
        [Display(Name = "Monto Ofertado")]
        public decimal MontoOfertado { get; set; }

        [Required(ErrorMessage = "La propiedad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una propiedad válida")]
        [Display(Name = "Propiedad")]
        public int PropertyId { get; set; }

        /// <summary>
        /// Archivo de carta de pre-aprobación bancaria (PDF o imagen).
        /// </summary>
        [Display(Name = "Carta de Pre-aprobación Bancaria")]
        public IFormFile? PreApprovalLetter { get; set; }

        public string? PreApprovalLetterUrl { get; set; }

        [MaxLength(1000, ErrorMessage = "Las notas no pueden exceder de 1000 caracteres.")]
        public string? Notes { get; set; }

        // Campos de solo lectura para mostrar en la vista
        public string? PropertyCode { get; set; }
        public decimal? PropertyPrice { get; set; }
    }
}
