using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de ofertas con la Regla de Negocio Atómica y sistema de Contra-Ofertas (Ítem 2.1).
    /// </summary>
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserActivityService _userActivityService;
        private readonly ICommissionService _commissionService;
        private readonly IMapper _mapper;

        public OfferService(
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository,
            IUserActivityService userActivityService,
            ICommissionService commissionService,
            IMapper mapper)
        {
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
            _userActivityService = userActivityService;
            _commissionService = commissionService;
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

        public async Task<List<OfferViewModel>> GetByPropertyIds(IEnumerable<int> propertyIds)
        {
            var offers = await _offerRepository.GetByPropertyIdsAsync(propertyIds);
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
                throw new NotFoundException("La propiedad no existe");

            if (property.Status != PropertyStatus.Available)
                throw new ValidationException("La propiedad no está disponible para ofertas");

            var offer = _mapper.Map<Offer>(vm);
            offer.ClienteId = clienteId;
            offer.FechaOferta = DateTime.UtcNow;
            offer.Status = OfferStatus.Pending;

            await _offerRepository.AddAsync(offer);

            vm.Id = offer.Id;
            return vm;
        }

        public async Task AcceptOffer(int offerId)
        {
            await _offerRepository.AcceptOfferTransactionAsync(offerId);

            await _commissionService.CreateForAcceptedOfferAsync(offerId);
        }

        public async Task RejectOffer(int offerId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            if (offer.Status != OfferStatus.Pending)
                throw new ValidationException("Solo se pueden rechazar ofertas pendientes");

            offer.Status = OfferStatus.Rejected;
            await _offerRepository.UpdateAsync(offer);
        }

        public async Task CounterOffer(int offerId, decimal counterAmount, string? counterMessage, string agentUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            if (offer.Status != OfferStatus.Pending)
                throw new ValidationException("Solo se pueden realizar contra-ofertas sobre propuestas pendientes");

            if (counterAmount <= 0)
                throw new ValidationException("El monto de la contra-oferta debe ser mayor a cero");

            offer.Status = OfferStatus.CounterOffered;
            offer.CounterOfferAmount = counterAmount;
            offer.CounterOfferMessage = counterMessage;
            offer.CounterOfferDate = DateTime.UtcNow;

            await _offerRepository.UpdateAsync(offer);

            await _userActivityService.LogActivityAsync(
                agentUserId,
                "Contra-Oferta Enviada",
                $"Envió una contra-oferta por RD$ {counterAmount:N0} al cliente",
                "bi-arrow-left-right text-info",
                "/Agent/Offers"
            );
        }

        public async Task AcceptCounterOffer(int offerId, string clientUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            if (offer.ClienteId != clientUserId)
                throw new ValidationException("No tiene permisos para modificar esta oferta");

            if (offer.Status != OfferStatus.CounterOffered)
                throw new ValidationException("Esta oferta no tiene una contra-oferta pendiente de aceptación");

            // Aceptar la propuesta ejecutando la transacción atómica
            await _offerRepository.AcceptOfferTransactionAsync(offerId);

            await _commissionService.CreateForAcceptedOfferAsync(offerId);

            await _userActivityService.LogActivityAsync(
                clientUserId,
                "Contra-Oferta Aceptada",
                $"Aceptó la contra-oferta del agente por RD$ {offer.CounterOfferAmount:N0}",
                "bi-check-circle-fill text-success",
                "/Offers/MyOffers"
            );
        }

        public async Task RejectCounterOffer(int offerId, string clientUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            if (offer.ClienteId != clientUserId)
                throw new ValidationException("No tiene permisos para modificar esta oferta");

            if (offer.Status != OfferStatus.CounterOffered)
                throw new ValidationException("Esta oferta no tiene una contra-oferta pendiente de resolución");

            offer.Status = OfferStatus.Rejected;
            await _offerRepository.UpdateAsync(offer);

            await _userActivityService.LogActivityAsync(
                clientUserId,
                "Contra-Oferta Rechazada",
                $"Rechazó la contra-oferta propuesta por el agente",
                "bi-x-circle-fill text-danger",
                "/Offers/MyOffers"
            );
        }
    }
}
