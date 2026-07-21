using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IOfferRepository : IGenericRepository<Offer>
    {
        /// <summary>
        /// Obtiene todas las ofertas de una propiedad específica.
        /// </summary>
        Task<List<Offer>> GetByPropertyIdAsync(int propertyId);

        /// <summary>
        /// Obtiene todas las ofertas realizadas por un cliente.
        /// </summary>
        Task<List<Offer>> GetByClienteIdAsync(string clienteId);
    }
}
