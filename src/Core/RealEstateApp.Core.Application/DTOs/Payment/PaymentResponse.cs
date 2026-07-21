using System;

namespace RealEstateApp.Core.Application.DTOs.Payment
{
    /// <summary>
    /// DTO de respuesta del procesamiento de pago.
    /// </summary>
    public class PaymentResponse
    {
        /// <summary>
        /// Indica si el pago fue procesado exitosamente.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Identificador único de la transacción generada por la pasarela.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Mensaje descriptivo del resultado de la transacción.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora en que se procesó la transacción.
        /// </summary>
        public DateTime ProcessedAt { get; set; }

        /// <summary>
        /// Monto procesado en la transacción.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Indica si hubo un error en el procesamiento.
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Descripción del error, si lo hubo.
        /// </summary>
        public string? Error { get; set; }
    }
}
