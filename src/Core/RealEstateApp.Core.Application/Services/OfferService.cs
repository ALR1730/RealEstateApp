using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;

        public OfferService(
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository,
            IUserActivityService userActivityService,
            ICommissionService commissionService,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _offerRepository = offerRepository ?? throw new ArgumentNullException(nameof(offerRepository));
            _propertyRepository = propertyRepository ?? throw new ArgumentNullException(nameof(propertyRepository));
            _userActivityService = userActivityService ?? throw new ArgumentNullException(nameof(userActivityService));
            _commissionService = commissionService ?? throw new ArgumentNullException(nameof(commissionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
            var property = await _propertyRepository.GetByIdAsync(vm.PropertyId);
            if (property == null)
                throw new NotFoundException("La propiedad no existe");

            if (property.Status != PropertyStatus.Available)
                throw new ValidationException("La propiedad no está disponible para ofertas");

            var offer = _mapper.Map<Offer>(vm);
            offer.ClienteId = clienteId;
            offer.FechaOferta = DateTime.UtcNow;
            offer.SetOfferAmount(vm.MontoOfertado);

            await _offerRepository.AddAsync(offer);

            vm.Id = offer.Id;
            return vm;
        }

        public async Task AcceptOffer(int offerId, string agentUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            await ValidateAgentOwnsOfferPropertyAsync(offer, agentUserId);

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                offer.Accept();
                await FinalizeAcceptedOfferAsync(offer);
            });
        }

        public async Task RejectOffer(int offerId, string agentUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            await ValidateAgentOwnsOfferPropertyAsync(offer, agentUserId);

            offer.Reject();
            await _offerRepository.UpdateAsync(offer);
        }

        public async Task CounterOffer(int offerId, decimal counterAmount, string? counterMessage, string agentUserId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            await ValidateAgentOwnsOfferPropertyAsync(offer, agentUserId);

            offer.MakeCounterOffer(counterAmount, counterMessage);

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

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                offer.AcceptCounterOffer(clientUserId);
                await FinalizeAcceptedOfferAsync(offer);
            });

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

            offer.RejectCounterOffer(clientUserId);
            await _offerRepository.UpdateAsync(offer);

            await _userActivityService.LogActivityAsync(
                clientUserId,
                "Contra-Oferta Rechazada",
                "Rechazó la contra-oferta propuesta por el agente",
                "bi-x-circle-fill text-danger",
                "/Offers/MyOffers"
            );
        }

        private async Task FinalizeAcceptedOfferAsync(Offer acceptedOffer)
        {
            await _offerRepository.UpdateAsync(acceptedOffer);

            var property = await _propertyRepository.FindByIdAsync(acceptedOffer.PropertyId);
            if (property != null)
            {
                property.MarkAsSold();
                await _propertyRepository.UpdateAsync(property);
            }

            var competingOffers = await _offerRepository.GetActiveCompetingOffersAsync(
                acceptedOffer.PropertyId,
                acceptedOffer.Id);

            if (competingOffers.Count > 0)
            {
                foreach (var otherOffer in competingOffers)
                {
                    otherOffer.RejectOnPropertySold();
                }

                await _offerRepository.UpdateRangeAsync(competingOffers);
            }

            await _commissionService.CreateForAcceptedOfferAsync(acceptedOffer.Id);
        }

        private async Task ValidateAgentOwnsOfferPropertyAsync(Offer offer, string agentUserId)
        {
            if (string.IsNullOrWhiteSpace(agentUserId))
                throw new ValidationException("No tiene permisos para gestionar ofertas de esta propiedad.");

            var ownerAgentId = await _propertyRepository.GetAgentIdByPropertyIdAsync(offer.PropertyId);
            if (ownerAgentId == null)
                throw new NotFoundException("La propiedad asociada a la oferta no existe.");

            if (ownerAgentId != agentUserId)
                throw new ValidationException("No tiene permisos para gestionar ofertas de esta propiedad.");
        }
    }
}
