using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.DTOs.Payment
{
    /// <summary>
    /// DTO para solicitar un procesamiento de pago (cobro de separación).
    /// </summary>
    public class PaymentRequest
    {
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La moneda es requerida")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "La moneda debe tener 3 caracteres (ej: USD, DOP)")]
        public string Currency { get; set; } = "DOP";

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID del cliente es requerido")]
        public string ClienteId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ID de la propiedad es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID de la propiedad no es válido")]
        public int PropertyId { get; set; }
    }
}
