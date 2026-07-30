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
        /// Ejecuta de forma atómica bajo una transacción explícita la aceptación de una oferta,
        /// la actualización de la propiedad a "Vendida" y el rechazo masivo del resto de ofertas pendientes.
        /// </summary>
        Task AcceptOfferTransactionAsync(int offerId);
    }
}
