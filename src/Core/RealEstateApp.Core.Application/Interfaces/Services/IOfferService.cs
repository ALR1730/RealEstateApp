using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Offer;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para gestión de ofertas.
    /// Incluye la regla de negocio atómica: al aceptar una oferta,
    /// se marca la propiedad como Vendida y se rechazan las demás ofertas.
    /// </summary>
    public interface IOfferService
    {
        Task<List<OfferViewModel>> GetAllViewModel();
        Task<List<OfferViewModel>> GetByPropertyId(int propertyId);
        Task<List<OfferViewModel>> GetByPropertyIds(IEnumerable<int> propertyIds);
        Task<List<OfferViewModel>> GetByClienteId(string clienteId);
        Task<SaveOfferViewModel> Add(SaveOfferViewModel vm, string clienteId);

        /// <summary>
        /// Acepta una oferta y ejecuta la regla de negocio atómica:
        /// 1. Marca la propiedad como "Vendida"
        /// 2. Rechaza en cascada todas las demás ofertas de esa propiedad
        /// 3. Bloquea nuevas propuestas
        /// </summary>
        Task AcceptOffer(int offerId);

        /// <summary>
        /// Rechaza una oferta individual.
        /// </summary>
        Task RejectOffer(int offerId);
    }
}
