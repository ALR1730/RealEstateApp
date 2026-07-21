using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Payment;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Contrato del servicio de pasarela de pagos.
    /// Gestiona el cobro del monto de separación al aceptar o proponer una separación.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Procesa un cobro de pago (separación del inmueble).
        /// </summary>
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);

        /// <summary>
        /// Procesa un reembolso de una transacción previamente cobrada.
        /// </summary>
        Task<PaymentResponse> RefundPaymentAsync(string transactionId);
    }
}
