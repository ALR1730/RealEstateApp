using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using RealEstateApp.Core.Application.DTOs.AiSearch;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services.AiSearch;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Application.Services
{
    public class AiSearchService : IAiSearchService
    {
        private const string CatalogCacheKey = "AiSearch:Catalogs:v1";
        private static readonly TimeSpan CatalogCacheDuration = TimeSpan.FromMinutes(5);

        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;
        private readonly IProvinceService _provinceService;
        private readonly IMapper _mapper;
        private readonly IMemoryCache? _memoryCache;
        private readonly IReadOnlyList<IAiQueryEntityExtractor> _extractors;

        public AiSearchService(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IProvinceService provinceService,
            IMapper mapper,
            IMemoryCache? memoryCache = null,
            IEnumerable<IAiQueryEntityExtractor>? extractors = null)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _provinceService = provinceService;
            _mapper = mapper;
            _memoryCache = memoryCache;
            _extractors = extractors?.ToList() ?? CreateDefaultExtractors();
        }

        public async Task<AiSearchInterpretationDto> SearchByNaturalLanguageAsync(string query)
        {
            var interpretation = await InterpretQueryAsync(query);

            var propertyViewModels = await _propertyService.GetAllWithFilters(interpretation.ParsedFilter);
            if (propertyViewModels != null)
            {
                propertyViewModels = propertyViewModels.Where(p => p.Status != PropertyStatus.Sold).ToList();
                var propertyDtos = _mapper.Map<List<PropertyDto>>(propertyViewModels);
                interpretation.Properties = propertyDtos;
                interpretation.MatchedPropertiesCount = propertyDtos.Count;
            }

            return interpretation;
        }

        public async Task<AiSearchInterpretationDto> InterpretQueryAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new AiSearchInterpretationDto
                {
                    OriginalQuery = query ?? string.Empty,
                    Explanation = "Consulta vacía. Mostrando todas las propiedades disponibles.",
                    ConfidenceScore = 0.0,
                    ParsedFilter = new PropertyFilterViewModel()
                };
            }

            var cleanQuery = query.Trim();
            var normalized = AiSearchTextHelper.NormalizeText(cleanQuery);
            var catalog = await GetCatalogSnapshotAsync();
            var context = new AiExtractionContext(normalized, catalog);

            foreach (var extractor in _extractors)
            {
                extractor.Extract(context);
            }

            return new AiSearchInterpretationDto
            {
                OriginalQuery = cleanQuery,
                Explanation = AiExplanationBuilder.Build(context.Entities),
                ConfidenceScore = CalculateConfidenceScore(context.MatchedSignals),
                ExtractedEntities = context.Entities,
                ParsedFilter = context.Filter
            };
        }

        public List<string> GetPromptSuggestions(string? lang = "es")
        {
            if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
            {
                return new List<string>
                {
                    "3 bedroom apartment in Bella Vista under RD$ 8M",
                    "Villa in Punta Cana with pool and virtual tour",
                    "4 bedroom house in Santiago with yard",
                    "Penthouse in Piantini with terrace and elevator",
                    "Apartment for rent with 2 bathrooms and gym under 45k"
                };
            }

            return new List<string>
            {
                "Apartamento de 3 habitaciones en Bella Vista con balcón por menos de RD$ 8M",
                "Villa en Punta Cana con piscina y tour virtual",
                "Casa en Santiago de 4 habitaciones con patio",
                "Penthouse en Piantini con terraza y ascensor",
                "Apartamento en alquiler con 2 baños y gimnasio por menos de 45 mil pesos"
            };
        }

        private async Task<AiSearchCatalogSnapshot> GetCatalogSnapshotAsync()
        {
            if (_memoryCache == null)
            {
                return await LoadCatalogSnapshotAsync();
            }

            var cached = await _memoryCache.GetOrCreateAsync(CatalogCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CatalogCacheDuration;
                return await LoadCatalogSnapshotAsync();
            });

            return cached ?? await LoadCatalogSnapshotAsync();
        }

        private async Task<AiSearchCatalogSnapshot> LoadCatalogSnapshotAsync()
        {
            var propertyTypes = await _propertyTypeService.GetAllViewModel();
            var saleTypes = await _saleTypeService.GetAllViewModel();
            var improvements = await _improvementService.GetAllViewModel();
            var provinces = await _provinceService.GetAllWithMunicipalitiesAsync();

            return new AiSearchCatalogSnapshot(propertyTypes, saleTypes, improvements, provinces);
        }

        private static double CalculateConfidenceScore(int matchedSignals) => matchedSignals switch
        {
            >= 4 => 0.98,
            3 => 0.90,
            2 => 0.80,
            1 => 0.65,
            _ => 0.40
        };

        private static IReadOnlyList<IAiQueryEntityExtractor> CreateDefaultExtractors() => new IAiQueryEntityExtractor[]
        {
            new PropertyTypeExtractor(),
            new SaleTypeExtractor(),
            new RoomsAndBathroomsExtractor(),
            new PriceRangeExtractor(),
            new LocationExtractor(),
            new ImprovementExtractor(),
            new SpecialCriteriaExtractor()
        };
    }
}
