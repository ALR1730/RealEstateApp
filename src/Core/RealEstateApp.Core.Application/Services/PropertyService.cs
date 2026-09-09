using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para propiedades.
    /// CRUD, gestión de imágenes, filtros combinados, búsqueda por código, historial de precios y gestión por agente.
    /// </summary>
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IPropertyPriceHistoryRepository _priceHistoryRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ICurrencyService _currencyService;
        private readonly ISavedSearchService _savedSearchService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        private const int MaxOwnerProperties = 2;

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
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyTypeRepository = propertyTypeRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
            _priceHistoryRepository = priceHistoryRepository;
            _fileStorageService = fileStorageService;
            _currencyService = currencyService;
            _savedSearchService = savedSearchService;
            _subscriptionService = subscriptionService;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task<List<PropertyViewModel>> GetAllViewModel()
        {
            var properties = await _propertyRepository.GetAllAsync();
            var list = _mapper.Map<List<PropertyViewModel>>(properties);
            await EnrichPropertiesWithCurrencyAsync(list);
            return list;
        }

        public async Task<PropertyViewModel?> GetByIdViewModel(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return null;
            var vm = _mapper.Map<PropertyViewModel>(property);

            // Obtener el historial completo de precios
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

                // Evaluar si el último cambio fue una rebaja
                var latestChange = histories.OrderByDescending(h => h.ChangeDate).FirstOrDefault(h => h.OldPrice > 0);
                if (latestChange != null && latestChange.NewPrice < latestChange.OldPrice)
                {
                    vm.HasPriceDrop = true;
                    vm.PriceDropPercentage = Math.Abs(latestChange.PercentageChange);
                    vm.PriceDropAmount = latestChange.OldPrice - latestChange.NewPrice;
                }
            }

            await EnrichPropertiesWithCurrencyAsync(new List<PropertyViewModel> { vm });
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
            // Validar límite de propiedades según el rol del usuario
            if (!string.IsNullOrEmpty(vm.AgentId))
            {
                var existingProperties = await _propertyRepository.GetByAgentIdAsync(vm.AgentId);
                var existingCount = existingProperties.Count;

                // Verificar si el usuario tiene rol Owner (límite máximo de 2 propiedades)
                var ownerSubscription = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(vm.AgentId);
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
                    // Para Agentes: validar según plan de suscripción
                    var canCreate = await _subscriptionService.CanAgentCreatePropertyAsync(vm.AgentId);
                    if (!canCreate)
                    {
                        var maxAllowed = ownerSubscription?.MaxActiveProperties ?? 3;
                        throw new ValidationException(
                            $"Has alcanzado el límite de {maxAllowed} propiedad(es) permitida(s) por tu plan de suscripción. " +
                            $"Actualmente tienes {existingCount} propiedad(es) activa(s). Actualiza tu plan para publicar más inmuebles.");
                    }
                }
            }

            var property = _mapper.Map<Property>(vm);
            property.Name = vm.Name;

            // Obtener el tipo de propiedad para generar dinámicamente su prefijo/identificador
            var propertyType = await _propertyTypeRepository.GetByIdAsync(vm.PropertyTypeId);
            string prefix = GetPropertyTypePrefix(propertyType?.Name);

            // Autogenerar código único con prefijo de tipo de propiedad
            property.Code = await GenerateUniqueCodeAsync(prefix);
            property.Status = PropertyStatus.Available;
            property.AgentId = vm.AgentId;

            // Guardar entidad de propiedad
            property = await _propertyRepository.AddAsync(property);

            // Registrar hito inicial en el Historial de Precios
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
                    ChangedByUserId = vm.AgentId,
                    ChangeReason = "Precio inicial de publicación"
                });
            }
            catch
            {
                // Silenciar error en auditoría secundaria para no abortar creación
            }

            // Guardar mejoras seleccionadas
            if (vm.ImprovementIds != null && vm.ImprovementIds.Count > 0)
            {
                await _propertyImprovementRepository.UpdatePropertyImprovementsAsync(property.Id, vm.ImprovementIds);
            }

            // Guardar imágenes si fueron subidas en lote
            if (vm.Files != null && vm.Files.Count > 0)
            {
                var imageEntities = new List<PropertyImage>();
                foreach (var file in vm.Files.Take(15))
                {
                    if (file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, "properties");
                        imageEntities.Add(new PropertyImage
                        {
                            PropertyId = property.Id,
                            ImageUrl = imageUrl
                        });
                    }
                }
                if (imageEntities.Any())
                {
                    await _propertyImageRepository.AddRangeAsync(imageEntities);
                }
            }

            // Disparar alertas automáticas a clientes con búsquedas guardadas coincidentes
            try
            {
                await _savedSearchService.CheckAndNotifyMatchesAsync(property);
            }
            catch
            {
                // Silenciar excepciones en notificaciones secundarias
            }

            var result = _mapper.Map<SavePropertyViewModel>(property);
            return result;
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

            // Si el precio cambió, registrarlo en el Historial de Precios
            if (priceChanged && oldPrice > 0)
            {
                try
                {
                    decimal percentageChange = 0;
                    if (oldPrice > 0)
                    {
                        percentageChange = Math.Round(((vm.Price - oldPrice) / oldPrice) * 100, 2);
                    }

                    var reason = vm.Price < oldPrice 
                        ? $"Rebaja de precio ({Math.Abs(percentageChange):0.0}%)" 
                        : (vm.Price > oldPrice ? $"Aumento de precio (+{percentageChange:0.0}%)" : "Ajuste de moneda");

                    await _priceHistoryRepository.AddAsync(new PropertyPriceHistory
                    {
                        PropertyId = id,
                        OldPrice = oldPrice,
                        NewPrice = vm.Price,
                        Currency = newCurrency,
                        PercentageChange = percentageChange,
                        ChangeDate = DateTime.UtcNow,
                        ChangedByUserId = vm.AgentId,
                        ChangeReason = reason
                    });
                }
                catch
                {
                    // Silenciar excepción en auditoría para asegurar continuidad
                }
            }

            // Actualizar mejoras asociadas
            if (vm.ImprovementIds != null)
            {
                await _propertyImprovementRepository.UpdatePropertyImprovementsAsync(id, vm.ImprovementIds);
            }

            // Si se subieron nuevas imágenes
            if (vm.Files != null && vm.Files.Count > 0)
            {
                var existingImages = await _propertyImageRepository.GetByPropertyIdAsync(id);
                int remainingSlots = Math.Max(0, 15 - existingImages.Count);

                var imageEntities = new List<PropertyImage>();
                foreach (var file in vm.Files.Take(remainingSlots))
                {
                    if (file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, "properties");
                        imageEntities.Add(new PropertyImage
                        {
                            PropertyId = id,
                            ImageUrl = imageUrl
                        });
                    }
                }
                if (imageEntities.Any())
                {
                    await _propertyImageRepository.AddRangeAsync(imageEntities);
                }
            }
        }

        public async Task Delete(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {id}");

            // Eliminar archivos físicos de imágenes
            var images = await _propertyImageRepository.GetByPropertyIdAsync(id);
            foreach (var img in images)
            {
                await _fileStorageService.DeleteFileAsync(img.ImageUrl, "properties");
            }

            await _propertyRepository.DeleteAsync(property);
        }

        public async Task DeleteImage(int imageId)
        {
            var image = await _propertyImageRepository.GetByIdAsync(imageId);
            if (image != null)
            {
                await _fileStorageService.DeleteFileAsync(image.ImageUrl, "properties");
                await _propertyImageRepository.DeleteAsync(image);
            }
        }

        public async Task<List<PropertyViewModel>> GetAllWithFilters(PropertyFilterViewModel filters)
        {
            var properties = await _propertyRepository.GetWithFiltersAsync(filters);
            var list = _mapper.Map<List<PropertyViewModel>>(properties);
            await EnrichPropertiesWithCurrencyAsync(list);
            await EnrichAgentNamesAsync(list);
            return list;
        }

        public async Task<List<PropertyViewModel>> GetByAgentId(string agentId)
        {
            var properties = await _propertyRepository.GetByAgentIdAsync(agentId);
            var list = _mapper.Map<List<PropertyViewModel>>(properties);
            await EnrichPropertiesWithCurrencyAsync(list);
            await EnrichAgentNamesAsync(list);
            return list;
        }

        public async Task<PropertyViewModel?> GetByCode(string code)
        {
            var property = await _propertyRepository.GetByCodeAsync(code);
            if (property == null) return null;
            var vm = _mapper.Map<PropertyViewModel>(property);
            await EnrichPropertiesWithCurrencyAsync(new List<PropertyViewModel> { vm });
            await EnrichAgentNamesAsync(new List<PropertyViewModel> { vm });
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

        private async Task EnrichPropertiesWithCurrencyAsync(List<PropertyViewModel> viewModels)
        {
            if (viewModels == null || viewModels.Count == 0) return;
            var exchangeRate = await _currencyService.GetExchangeRateAsync();

            foreach (var vm in viewModels)
            {
                if (string.IsNullOrEmpty(vm.Currency))
                {
                    vm.Currency = CurrencyConstants.DOP;
                }

                if (string.Equals(vm.Currency, CurrencyConstants.USD, StringComparison.OrdinalIgnoreCase))
                {
                    vm.PriceInUSD = vm.Price;
                    vm.PriceInDOP = vm.Price * exchangeRate;
                }
                else
                {
                    vm.PriceInDOP = vm.Price;
                    vm.PriceInUSD = exchangeRate > 0 ? Math.Round(vm.Price / exchangeRate, 2) : vm.Price;
                }

                vm.DisplayPrice = _currencyService.FormatPrice(vm.Price, vm.Currency);
                vm.DisplaySecondaryPrice = _currencyService.FormatSecondaryPrice(vm.Price, vm.Currency, vm.Currency, exchangeRate);

                // Detectar si hubo rebaja de precio
                if (!vm.HasPriceDrop)
                {
                    try
                    {
                        var latestHistory = await _priceHistoryRepository.GetLatestByPropertyIdAsync(vm.Id);
                        if (latestHistory != null && latestHistory.OldPrice > 0 && latestHistory.NewPrice < latestHistory.OldPrice)
                        {
                            vm.HasPriceDrop = true;
                            vm.PriceDropPercentage = Math.Abs(latestHistory.PercentageChange);
                            vm.PriceDropAmount = latestHistory.OldPrice - latestHistory.NewPrice;
                        }
                    }
                    catch
                    {
                        // Silenciar error secundario
                    }
                }
            }
        }

        private async Task EnrichAgentNamesAsync(List<PropertyViewModel> viewModels)
        {
            if (viewModels == null || viewModels.Count == 0) return;

            var agentIds = viewModels
                .Where(v => !string.IsNullOrWhiteSpace(v.AgentId))
                .Select(v => v.AgentId)
                .Distinct()
                .ToList();

            if (agentIds.Count == 0) return;

            Dictionary<string, AccountUserDto>? userDict = null;
            try
            {
                userDict = await _accountService.GetUsersByIdsAsync(agentIds);
            }
            catch
            {
                return;
            }

            if (userDict == null || userDict.Count == 0) return;

            foreach (var vm in viewModels)
            {
                if (!string.IsNullOrWhiteSpace(vm.AgentId) &&
                    userDict.TryGetValue(vm.AgentId, out var agent))
                {
                    vm.AgentName = $"{agent.FirstName} {agent.LastName}".Trim();
                }
            }
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
                // Desactivar destacado
                property.IsFeatured = false;
                property.FeaturedUntil = null;
            }
            else
            {
                // Activar destacado
                property.IsFeatured = true;
                property.FeaturedUntil = DateTime.UtcNow.AddDays(durationDays);
            }

            await _propertyRepository.UpdateAsync(property);
        }

        public async Task<List<string>> GetDistinctSectorsAsync(int? provinceId = null, int? municipalityId = null)
        {
            return await _propertyRepository.GetDistinctSectorsAsync(provinceId, municipalityId);
        }

        private async Task<string> GenerateUniqueCodeAsync(string prefix)
        {
            string code;
            do
            {
                code = $"{prefix}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }
            while (await _propertyRepository.GetByCodeAsync(code) != null);

            return code;
        }

        private string GetPropertyTypePrefix(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "PROP";

            var trimmed = typeName.Trim();
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length > 1)
            {
                var initials = new string(words.Select(w => char.ToUpper(w[0])).ToArray());
                if (initials.Length >= 3)
                    return initials[..3];

                var firstWordChar = char.ToUpper(words[0][0]);
                var secondWordClean = new string(words[1].Where(char.IsLetterOrDigit).ToArray()).ToUpper();
                if (secondWordClean.Length >= 2)
                    return $"{firstWordChar}{secondWordClean[..2]}";

                return (firstWordChar + secondWordClean).PadRight(3, 'X');
            }

            var cleanWord = new string(trimmed.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            if (cleanWord.Length <= 3)
                return cleanWord.PadRight(3, 'X');

            // Coincidencias conocidas comunes para alta legibilidad
            if (cleanWord.StartsWith("APARTAM")) return "APT";
            if (cleanWord.StartsWith("VILL")) return "VIL";
            if (cleanWord.StartsWith("CASA")) return "CAS";
            if (cleanWord.StartsWith("PENTH")) return "PNT";
            if (cleanWord.StartsWith("TERRE")) return "TER";
            if (cleanWord.StartsWith("LOCAL")) return "LOC";
            if (cleanWord.StartsWith("EDIFI")) return "EDI";

            // Algoritmo dinámico para cualquier tipo de propiedad nuevo
            var firstChar = cleanWord[0];
            var consonants = cleanWord.Substring(1).Where(c => !"AEIOUáéíóúÁÉÍÓÚ".Contains(c)).ToArray();
            if (consonants.Length >= 2)
            {
                return $"{firstChar}{consonants[0]}{consonants[1]}";
            }

            return cleanWord[..3];
        }
    }
}
