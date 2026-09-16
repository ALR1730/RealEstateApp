using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Review;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IReviewService
    {
        /// <summary>
        /// Envía una reseña del cliente hacia un agente, validando que exista
        /// una transacción completada (oferta aceptada) y que no se duplique.
        /// </summary>
        Task SubmitReviewAsync(SaveAgentReviewViewModel vm, string clienteId);

        /// <summary>
        /// Indica si el cliente puede reseñar (tiene transacción completada).
        /// </summary>
        Task<bool> CanReviewAsync(string clienteId, string agentId, int propertyId);

        /// <summary>
        /// Indica si el cliente ya reseñó a este agente por esta propiedad.
        /// </summary>
        Task<bool> HasReviewedAsync(string clienteId, string agentId, int propertyId);

        /// <summary>
        /// Obtiene las reseñas públicas de un agente.
        /// </summary>
        Task<List<AgentReviewViewModel>> GetReviewsByAgentAsync(string agentId);

        /// <summary>
        /// Obtiene el resumen de calificaciones de un agente.
        /// </summary>
        Task<AgentReviewSummaryViewModel> GetAgentReviewSummaryAsync(string agentId);

        /// <summary>
        /// Obtiene todas las reseñas del sistema (para administración).
        /// </summary>
        Task<List<AgentReviewViewModel>> GetAllAsync();
    }
}
