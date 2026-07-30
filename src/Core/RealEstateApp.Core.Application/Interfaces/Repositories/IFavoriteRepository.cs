using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IFavoriteRepository : IGenericRepository<Favorite>
    {
        /// <summary>
        /// Obtiene todos los favoritos de un cliente.
        /// </summary>
        Task<List<Favorite>> GetByClienteIdAsync(string clienteId);

        /// <summary>
        /// Verifica si un favorito ya existe para el cliente y propiedad dados.
        /// </summary>
        Task<Favorite?> GetByClienteAndPropertyAsync(string clienteId, int propertyId);
        /// <summary>
        /// Obtiene todos los favoritos registrados para una propiedad.
        /// </summary>
        Task<List<Favorite>> GetByPropertyIdAsync(int propertyId);
    }
}
