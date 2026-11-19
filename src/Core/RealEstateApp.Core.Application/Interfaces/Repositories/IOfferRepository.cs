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

        /// <summary>
        /// Obtiene todas las ofertas de un listado de propiedades.
        /// </summary>
        Task<List<Offer>> GetByPropertyIdsAsync(IEnumerable<int> propertyIds);

        /// <summary>
        /// Obtiene las ofertas competidoras activas (Pending o CounterOffered) de una propiedad excluyendo la oferta aceptada.
        /// </summary>
        Task<List<Offer>> GetActiveCompetingOffersAsync(int propertyId, int excludeOfferId);
    }
}
