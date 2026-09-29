using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Helpers;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de aplicación principal para gestión de propiedades.
    /// Orquesta el ciclo de vida y delega responsabilidades a servicios especializados (SRP).
    /// </summary>
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IPropertyPriceHistoryRepository _priceHistoryRepository;
        private readonly ISavedSearchService _savedSearchService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPropertyImageService _propertyImageService;
        private readonly IPropertyEnrichmentService _propertyEnrichmentService;
        private readonly IMapper _mapper;
        private readonly ILogger<PropertyService> _logger;

        private const int MaxOwnerProperties = 2;

        /// <summary>
        /// Constructor principal utilizado por el contenedor de Inyección de Dependencias.
        /// </summary>
        public PropertyService(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyTypeRepository propertyTypeRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IPropertyPriceHistoryRepository priceHistoryRepository,
            ISavedSearchService savedSearchService,
            ISubscriptionService subscriptionService,
            IPropertyImageService propertyImageService,
            IPropertyEnrichmentService propertyEnrichmentService,
            IMapper mapper,
            ILogger<PropertyService>? logger = null)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyTypeRepository = propertyTypeRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _priceHistoryRepository = priceHistoryRepository;
            _savedSearchService = savedSearchService;
            _subscriptionService = subscriptionService;
            _propertyImageService = propertyImageService;
            _propertyEnrichmentService = propertyEnrichmentService;
            _mapper = mapper;
            _logger = logger ?? NullLogger<PropertyService>.Instance;
        }

        /// <summary>
        /// Constructor sobrecargado para retrocompatibilidad total con pruebas unitarias existentes.
        /// </summary>
        public PropertyService(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyTypeRepository propertyTypeRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IPropertyPriceHistoryRepository priceHistoryRepository,
            IFileStorageService fileStorageService,
            ICurrencyService currencyService,
            ISavedSearchService savedSearchService,
            ISubscriptionService subscriptionService,
            IAccountService accountService,
            IMapper mapper,
            ILogger<PropertyService>? logger = null)
            : this(
                propertyRepository,
                propertyImageRepository,
                propertyTypeRepository,
                propertyImprovementRepository,
                priceHistoryRepository,
                savedSearchService,
                subscriptionService,
                new PropertyImageService(propertyImageRepository, fileStorageService),
                new PropertyEnrichmentService(currencyService, accountService, priceHistoryRepository),
                mapper,
                logger)
        {
        }

        public async Task<List<PropertyViewModel>> GetAllViewModel()
        {
            var properties = await _propertyRepository.GetAllAsync();
            var list = _mapper.Map<List<PropertyViewModel>>(properties);
            await _propertyEnrichmentService.EnrichPropertiesWithCurrencyAsync(list);
            return list;
        }

        public async Task<PropertyViewModel?> GetByIdViewModel(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return null;

            var vm = _mapper.Map<PropertyViewModel>(property);

            var histories = await _priceHistoryRepository.GetByPropertyIdAsync(id);
            if (histories != null && histories.Count > 0)
            {
                vm.PriceHistories = histories.Select(h => new PriceHistoryViewModel
                {
                    Id = h.Id,
                    PropertyId = h.PropertyId,
                    OldPrice = h.OldPrice,
                    NewPrice = h.NewPrice,
                    Currency = h.Currency,
                    PercentageChange = h.PercentageChange,
                    ChangeDate = h.ChangeDate,
                    ChangedByUserId = h.ChangedByUserId,
                    ChangeReason = h.ChangeReason
                }).ToList();

                var initialHistory = histories.FirstOrDefault(h => h.OldPrice == 0);
                if (initialHistory != null)
                {
                    vm.OriginalPrice = initialHistory.NewPrice;
                }

                var latestChange = histories.OrderByDescending(h => h.ChangeDate).FirstOrDefault(h => h.OldPrice > 0);
                if (latestChange != null && latestChange.NewPrice < latestChange.OldPrice)
                {
                    vm.HasPriceDrop = true;
                    vm.PriceDropPercentage = Math.Abs(latestChange.PercentageChange);
                    vm.PriceDropAmount = latestChange.OldPrice - latestChange.NewPrice;
                }
            }

            await _propertyEnrichmentService.EnrichPropertiesWithCurrencyAsync(new List<PropertyViewModel> { vm });
            return vm;
        }

        public async Task<SavePropertyViewModel?> GetByIdSaveViewModel(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return null;

            var vm = _mapper.Map<SavePropertyViewModel>(property);
            var images = await _propertyImageRepository.GetByPropertyIdAsync(id);
            vm.ExistingImages = images.Select(i => i.ImageUrl).ToList();

            var improvements = await _propertyImprovementRepository.GetByPropertyIdAsync(id);
            vm.ImprovementIds = improvements.Select(pi => pi.ImprovementId).ToList();

            return vm;
        }

        public async Task<SavePropertyViewModel> Add(SavePropertyViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.AgentId))
            {
                await ValidateAgentPropertyLimitsAsync(vm.AgentId);
            }

            var property = _mapper.Map<Property>(vm);
            property.Name = vm.Name;

            var propertyType = await _propertyTypeRepository.GetByIdAsync(vm.PropertyTypeId);
            string prefix = PropertyCodeGenerator.GetPropertyTypePrefix(propertyType?.Name);

            property.Code = await PropertyCodeGenerator.GenerateUniqueCodeAsync(
                prefix,
                async code => await _propertyRepository.GetByCodeAsync(code) != null);

            property.Status = PropertyStatus.Available;
            property.AgentId = vm.AgentId;

            property = await _propertyRepository.AddAsync(property);

            await RecordInitialPriceHistoryAsync(property, vm.AgentId);

            if (vm.ImprovementIds != null && vm.ImprovementIds.Count > 0)
            {
                await _propertyImprovementRepository.UpdatePropertyImprovementsAsync(property.Id, vm.ImprovementIds);
            }

            if (vm.Files != null && vm.Files.Count > 0)
            {
                await _propertyImageService.SaveImagesAsync(property.Id, vm.Files);
            }

            await NotifyMatchingSavedSearchesAsync(property);

            return _mapper.Map<SavePropertyViewModel>(property);
        }

        public async Task Update(SavePropertyViewModel vm, int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {id}");

            var oldPrice = property.Price;
            var oldCurrency = property.Currency;
            var newCurrency = !string.IsNullOrEmpty(vm.Currency) ? vm.Currency : CurrencyConstants.DOP;
            var priceChanged = oldPrice != vm.Price || !string.Equals(oldCurrency, newCurrency, StringComparison.OrdinalIgnoreCase);

            property.Name = vm.Name;
            property.Price = vm.Price;
            property.Currency = newCurrency;
            property.Rooms = vm.Rooms;
            property.Bathrooms = vm.Bathrooms;
            property.SizeInMeters = vm.SizeInMeters;
            property.Description = vm.Description;
            property.PropertyTypeId = vm.PropertyTypeId;
            property.SaleTypeId = vm.SaleTypeId;
            property.Latitude = vm.Latitude;
            property.Longitude = vm.Longitude;
            property.VideoUrl = vm.VideoUrl;
            property.Tour360Url = vm.Tour360Url;
            property.MatterportModelId = vm.MatterportModelId;
            property.MontoSeparacion = vm.MontoSeparacion;
            property.PorcentajeInicialRequerido = vm.PorcentajeInicialRequerido;
            property.IsFinanciable = vm.IsFinanciable;
            property.ProvinceId = vm.ProvinceId;
            property.MunicipalityId = vm.MunicipalityId;
            property.Sector = vm.Sector;
            property.FullAddress = vm.FullAddress;
            property.IsFeatured = vm.IsFeatured;
            property.FeaturedUntil = vm.FeaturedUntil;

            await _propertyRepository.UpdateAsync(property);

            if (priceChanged && oldPrice > 0)
            {
                await RecordPriceChangeHistoryAsync(id, oldPrice, vm.Price, newCurrency, vm.AgentId);
            }

            if (vm.ImprovementIds != null)
            {
                await _propertyImprovementRepository.UpdatePropertyImprovementsAsync(id, vm.ImprovementIds);
            }

            if (vm.Files != null && vm.Files.Count > 0)
            {
                await _propertyImageService.SaveImagesAsync(id, vm.Files);
            }
        }

        public async Task Delete(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {id}");

            await _propertyImageService.DeletePhysicalImagesAsync(property.Images);
            await _propertyRepository.DeleteAsync(property);
        }

        public async Task DeleteImage(int imageId)
        {
            await _propertyImageService.DeleteImageByIdAsync(imageId);
        }

        public async Task<List<PropertyViewModel>> GetAllWithFilters(PropertyFilterViewModel filters)
        {
            var properties = await _propertyRepository.GetWithFiltersAsync(filters);
            var list = _mapper.Map<List<PropertyViewModel>>(properties);
            await _propertyEnrichmentService.EnrichPropertiesWithCurrencyAsync(list);
            await _propertyEnrichmentService.EnrichAgentNamesAsync(list);
            return list;
        }

        public async Task<List<PropertyViewModel>> GetByAgentId(string agentId)
        {
            var properties = await _propertyRepository.GetByAgentIdAsync(agentId);
            var list = _mapper.Map<List<PropertyViewModel>>(properties);
            await _propertyEnrichmentService.EnrichPropertiesWithCurrencyAsync(list);
            await _propertyEnrichmentService.EnrichAgentNamesAsync(list);
            return list;
        }

        public async Task<PropertyViewModel?> GetByCode(string code)
        {
            var property = await _propertyRepository.GetByCodeAsync(code);
            if (property == null) return null;

            var vm = _mapper.Map<PropertyViewModel>(property);
            await _propertyEnrichmentService.EnrichPropertiesWithCurrencyAsync(new List<PropertyViewModel> { vm });
            await _propertyEnrichmentService.EnrichAgentNamesAsync(new List<PropertyViewModel> { vm });
            return vm;
        }

        public async Task<List<PriceHistoryViewModel>> GetPriceHistoryAsync(int propertyId)
        {
            var histories = await _priceHistoryRepository.GetByPropertyIdAsync(propertyId);
            return histories.Select(h => new PriceHistoryViewModel
            {
                Id = h.Id,
                PropertyId = h.PropertyId,
                OldPrice = h.OldPrice,
                NewPrice = h.NewPrice,
                Currency = h.Currency,
                PercentageChange = h.PercentageChange,
                ChangeDate = h.ChangeDate,
                ChangedByUserId = h.ChangedByUserId,
                ChangeReason = h.ChangeReason
            }).ToList();
        }

        public async Task ReassignAgent(int propertyId, string newAgentId)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {propertyId}");

            property.AgentId = newAgentId;
            await _propertyRepository.UpdateAsync(property);
        }

        public async Task ToggleFeaturedAsync(int propertyId, int durationDays = 30)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {propertyId}");

            if (property.IsFeatured && (!property.FeaturedUntil.HasValue || property.FeaturedUntil > DateTime.UtcNow))
            {
                property.IsFeatured = false;
                property.FeaturedUntil = null;
            }
            else
            {
                if (!string.IsNullOrEmpty(property.AgentId))
                {
                    var (allowed, message) = await _subscriptionService.CanAgentFeaturePropertyAsync(property.AgentId, property.Id);
                    if (!allowed)
                    {
                        throw new ValidationException(message);
                    }
                }

                property.IsFeatured = true;
                property.FeaturedUntil = DateTime.UtcNow.AddDays(durationDays);
            }

            await _propertyRepository.UpdateAsync(property);
        }

        public async Task<List<string>> GetDistinctSectorsAsync(int? provinceId = null, int? municipalityId = null)
        {
            return await _propertyRepository.GetDistinctSectorsAsync(provinceId, municipalityId);
        }

        #region Private Helper Methods

        private async Task ValidateAgentPropertyLimitsAsync(string agentId)
        {
            var existingProperties = await _propertyRepository.GetByAgentIdAsync(agentId);
            var existingCount = existingProperties.Count;

            var ownerSubscription = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(agentId);
            if (ownerSubscription != null && ownerSubscription.PlanName == "Owner")
            {
                if (existingCount >= MaxOwnerProperties)
                {
                    throw new ValidationException(
                        $"Como propietario directo puedes publicar un máximo de {MaxOwnerProperties} inmuebles simultáneos. " +
                        $"Actualmente tienes {existingCount} propiedad(es) activa(s).");
                }
            }
            else
            {
                var canCreate = await _subscriptionService.CanAgentCreatePropertyAsync(agentId);
                if (!canCreate)
                {
                    var maxAllowed = ownerSubscription?.MaxActiveProperties ?? 3;
                    throw new ValidationException(
                        $"Has alcanzado el límite de {maxAllowed} propiedad(es) permitida(s) por tu plan de suscripción. " +
                        $"Actualmente tienes {existingCount} propiedad(es) activa(s). Actualiza tu plan para publicar más inmuebles.");
                }
            }
        }

        private async Task RecordInitialPriceHistoryAsync(Property property, string? agentId)
        {
            try
            {
                await _priceHistoryRepository.AddAsync(new PropertyPriceHistory
                {
                    PropertyId = property.Id,
                    OldPrice = 0,
                    NewPrice = property.Price,
                    Currency = !string.IsNullOrEmpty(property.Currency) ? property.Currency : CurrencyConstants.DOP,
                    PercentageChange = 0,
                    ChangeDate = DateTime.UtcNow,
                    ChangedByUserId = agentId,
                    ChangeReason = "Precio inicial de publicación"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo no crítico al registrar historial inicial de precio para la propiedad con código {Code}", property.Code);
            }
        }

        private async Task RecordPriceChangeHistoryAsync(int propertyId, decimal oldPrice, decimal newPrice, string currency, string? agentId)
        {
            try
            {
                decimal percentageChange = oldPrice > 0
                    ? Math.Round(((newPrice - oldPrice) / oldPrice) * 100, 2)
                    : 0;

                var reason = newPrice < oldPrice
                    ? $"Rebaja de precio ({Math.Abs(percentageChange):0.0}%)"
                    : (newPrice > oldPrice ? $"Aumento de precio (+{percentageChange:0.0}%)" : "Ajuste de moneda");

                await _priceHistoryRepository.AddAsync(new PropertyPriceHistory
                {
                    PropertyId = propertyId,
                    OldPrice = oldPrice,
                    NewPrice = newPrice,
                    Currency = currency,
                    PercentageChange = percentageChange,
                    ChangeDate = DateTime.UtcNow,
                    ChangedByUserId = agentId,
                    ChangeReason = reason
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo no crítico al registrar cambio de precio en auditoría para la propiedad {PropertyId}", propertyId);
            }
        }

        private async Task NotifyMatchingSavedSearchesAsync(Property property)
        {
            try
            {
                await _savedSearchService.CheckAndNotifyMatchesAsync(property);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo no crítico al disparar alertas de búsquedas guardadas para la propiedad {PropertyId}", property.Id);
            }
        }

        #endregion
    }
}
