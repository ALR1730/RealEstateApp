using System;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Payment;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    /// <summary>
    /// Simulador de pasarela de pagos para desarrollo.
    /// En producción, reemplazar con un SDK real (Stripe, PayPal, etc.).
    /// Procesa el cobro del MontoSeparacion al aceptar/proponer una separación.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        /// <summary>
        /// Simula el procesamiento de un pago.
        /// Siempre retorna éxito en modo desarrollo.
        /// </summary>
        public Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            // Simulación: generar un TransactionId único y retornar éxito
            var response = new PaymentResponse
            {
                Success = true,
                TransactionId = $"TXN-{Guid.NewGuid():N}".ToUpper()[..20],
                Message = $"Pago de separación procesado exitosamente por {request.Amount:C} {request.Currency}",
                ProcessedAt = DateTime.UtcNow,
                Amount = request.Amount,
                HasError = false
            };

            Console.WriteLine($"[PaymentService] Pago simulado: {response.TransactionId} | " +
                              $"Monto: {request.Amount} {request.Currency} | " +
                              $"Cliente: {request.ClienteId} | Propiedad: {request.PropertyId}");

            return Task.FromResult(response);
        }

        /// <summary>
        /// Simula el reembolso de una transacción.
        /// </summary>
        public Task<PaymentResponse> RefundPaymentAsync(string transactionId)
        {
            var response = new PaymentResponse
            {
                Success = true,
                TransactionId = transactionId,
                Message = $"Reembolso procesado exitosamente para la transacción {transactionId}",
                ProcessedAt = DateTime.UtcNow,
                HasError = false
            };

            Console.WriteLine($"[PaymentService] Reembolso simulado: {transactionId}");

            return Task.FromResult(response);
        }
    }
}
