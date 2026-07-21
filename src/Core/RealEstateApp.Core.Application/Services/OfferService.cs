using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de ofertas con la Regla de Negocio Atómica:
    /// Al aceptar una oferta → propiedad pasa a "Vendida" → rechazo en cascada de las demás.
    /// </summary>
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public OfferService(
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper)
        {
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<List<OfferViewModel>> GetAllViewModel()
        {
            var offers = await _offerRepository.GetAllAsync();
            return _mapper.Map<List<OfferViewModel>>(offers);
        }

        public async Task<List<OfferViewModel>> GetByPropertyId(int propertyId)
        {
            var offers = await _offerRepository.GetByPropertyIdAsync(propertyId);
            return _mapper.Map<List<OfferViewModel>>(offers);
        }

        public async Task<List<OfferViewModel>> GetByClienteId(string clienteId)
        {
            var offers = await _offerRepository.GetByClienteIdAsync(clienteId);
            return _mapper.Map<List<OfferViewModel>>(offers);
        }

        public async Task<SaveOfferViewModel> Add(SaveOfferViewModel vm, string clienteId)
        {
            // Verificar que la propiedad existe y está disponible
            var property = await _propertyRepository.GetByIdAsync(vm.PropertyId);
            if (property == null)
                throw new Exception("La propiedad no existe");

            if (property.Status != "Disponible")
                throw new Exception("La propiedad no está disponible para ofertas");

            var offer = _mapper.Map<Offer>(vm);
            offer.ClienteId = clienteId;
            offer.FechaOferta = DateTime.UtcNow;
            offer.Status = OfferStatus.Pending;

            await _offerRepository.AddAsync(offer);

            return vm;
        }

        /// <summary>
        /// Regla de Negocio Atómica:
        /// 1. Aceptar la oferta seleccionada
        /// 2. Cambiar el estado de la propiedad a "Vendida"
        /// 3. Rechazar en cascada todas las demás ofertas pendientes de esa propiedad
        /// </summary>
        public async Task AcceptOffer(int offerId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new Exception("La oferta no existe");

            if (offer.Status != OfferStatus.Pending)
                throw new Exception("Solo se pueden aceptar ofertas pendientes");

            // 1. Aceptar la oferta
            offer.Status = OfferStatus.Accepted;
            await _offerRepository.UpdateAsync(offer);

            // 2. Cambiar la propiedad a "Vendida"
            var property = await _propertyRepository.GetByIdAsync(offer.PropertyId);
            if (property != null)
            {
                property.Status = "Vendida";
                await _propertyRepository.UpdateAsync(property);
            }

            // 3. Rechazar en cascada todas las demás ofertas pendientes
            var otherOffers = await _offerRepository.GetByPropertyIdAsync(offer.PropertyId);
            foreach (var otherOffer in otherOffers.Where(o => o.Id != offerId && o.Status == OfferStatus.Pending))
            {
                otherOffer.Status = OfferStatus.Rejected;
                await _offerRepository.UpdateAsync(otherOffer);
            }
        }

        public async Task RejectOffer(int offerId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new Exception("La oferta no existe");

            if (offer.Status != OfferStatus.Pending)
                throw new Exception("Solo se pueden rechazar ofertas pendientes");

            offer.Status = OfferStatus.Rejected;
            await _offerRepository.UpdateAsync(offer);
        }
    }
}
