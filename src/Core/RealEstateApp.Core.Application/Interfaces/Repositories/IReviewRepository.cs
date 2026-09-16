using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IReviewRepository : IGenericRepository<AgentReview>
    {
        /// <summary>
        /// Verifica si el cliente tiene una transacción completada (oferta aceptada)
        /// con el agente en la propiedad dada.
        /// </summary>
        Task<bool> HasCompletedTransactionAsync(string clienteId, string agentId, int propertyId);

        /// <summary>
        /// Verifica si el cliente ya reseñó al agente por esta propiedad.
        /// </summary>
        Task<bool> ExistsAsync(string clienteId, string agentId, int propertyId);

        /// <summary>
        /// Obtiene todas las reseñas de un agente (junto a la propiedad reseñada).
        /// </summary>
        Task<List<AgentReview>> GetByAgentIdAsync(string agentId);

        /// <summary>
        /// Obtiene todas las reseñas del sistema (para el administrador).
        /// </summary>
        Task<List<AgentReview>> GetAllWithDetailsAsync();
    }
}
