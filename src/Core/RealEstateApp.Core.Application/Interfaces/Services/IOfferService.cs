using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Offer;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para gestión de ofertas y contra-ofertas (Ítem 2.1).
    /// Incluye la regla de negocio atómica: al aceptar una oferta o contra-oferta,
    /// se marca la propiedad como Vendida/Reservada y se rechazan las demás ofertas.
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

        /// <summary>
        /// Realiza una contra-oferta a la propuesta recibida (Ítem 2.1).
        /// Cambia el estado de la oferta a CounterOffered y notifica al cliente.
        /// </summary>
        Task CounterOffer(int offerId, decimal counterAmount, string? counterMessage, string agentUserId);

        /// <summary>
        /// Permite al cliente aceptar la contra-oferta enviada por el agente.
        /// Ejecuta la regla atómica de venta.
        /// </summary>
        Task AcceptCounterOffer(int offerId, string clientUserId);

        /// <summary>
        /// Permite al cliente rechazar la contra-oferta del agente.
        /// </summary>
        Task RejectCounterOffer(int offerId, string clientUserId);
    }
}
