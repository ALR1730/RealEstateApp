using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IChatRepository : IGenericRepository<Chat>
    {
        /// <summary>
        /// Obtiene todos los mensajes de un hilo de chat por propiedad entre cliente y agente.
        /// </summary>
        Task<List<Chat>> GetChatThreadAsync(string clienteId, string agenteId, int? propertyId);

        /// <summary>
        /// Obtiene todos los hilos de chat de un usuario (como cliente o agente).
        /// </summary>
        Task<List<Chat>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Obtiene todos los mensajes de chat asociados a una propiedad específica.
        /// </summary>
        Task<List<Chat>> GetByPropertyIdAsync(int propertyId);
    }
}
