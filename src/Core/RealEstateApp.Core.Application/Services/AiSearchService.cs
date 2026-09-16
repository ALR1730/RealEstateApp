using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.DTOs.AiSearch;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using ImprovementVM = RealEstateApp.Core.Application.ViewModels.Improvement.ImprovementViewModel;
using PropertyTypeVM = RealEstateApp.Core.Application.ViewModels.PropertyType.PropertyTypeViewModel;
using SaleTypeVM = RealEstateApp.Core.Application.ViewModels.SaleType.SaleTypeViewModel;

namespace RealEstateApp.Core.Application.Services
{
    public class AiSearchService : IAiSearchService
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;
        private readonly IProvinceService _provinceService;
        private readonly IMapper _mapper;

        public AiSearchService(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IProvinceService provinceService,
            IMapper mapper)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _provinceService = provinceService;
            _mapper = mapper;
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
            var normalized = NormalizeText(cleanQuery);

            var filter = new PropertyFilterViewModel();
            var entities = new AiExtractedEntitiesDto();
            int matchedSignals = 0;

            // 1. Tipos de propiedad
            var propertyTypes = await _propertyTypeService.GetAllViewModel();
            DetectPropertyType(normalized, propertyTypes, filter, entities, ref matchedSignals);

            // 2. Tipos de venta (Venta / Alquiler)
            var saleTypes = await _saleTypeService.GetAllViewModel();
            DetectSaleType(normalized, saleTypes, filter, entities, ref matchedSignals);

            // 3. Habitaciones
            DetectRooms(normalized, filter, entities, ref matchedSignals);

            // 4. Baños
            DetectBathrooms(normalized, filter, entities, ref matchedSignals);

            // 5. Precios (Rango / Mínimo / Máximo)
            DetectPrice(normalized, filter, entities, ref matchedSignals);

            // 6. Ubicación (Provincias y Sectores)
            var provinces = await _provinceService.GetAllWithMunicipalitiesAsync();
            DetectLocation(normalized, provinces, filter, entities, ref matchedSignals);

            // 7. Mejoras / Amenidades
            var improvements = await _improvementService.GetAllViewModel();
            DetectImprovements(normalized, improvements, filter, entities, ref matchedSignals);

            // 8. Criterios Especiales (Tour 360, Financiable, Destacado)
            DetectSpecialCriteria(normalized, filter, entities, ref matchedSignals);

            // Calcular confianza
            double confidence = matchedSignals switch
            {
                >= 4 => 0.98,
                3 => 0.90,
                2 => 0.80,
                1 => 0.65,
                _ => 0.40
            };

            // Construir explicación en lenguaje natural
            string explanation = BuildExplanation(entities, filter);

            return new AiSearchInterpretationDto
            {
                OriginalQuery = cleanQuery,
                Explanation = explanation,
                ConfidenceScore = confidence,
                ExtractedEntities = entities,
                ParsedFilter = filter
            };
        }

        public List<string> GetPromptSuggestions()
        {
            return new List<string>
            {
                "Apartamento de 3 habitaciones en Bella Vista con balcón por menos de RD$ 8M",
                "Villa en Punta Cana con piscina y tour virtual",
                "Casa en Santiago de 4 habitaciones con patio",
                "Penthouse en Piantini con terraza y ascensor",
                "Apartamento en alquiler con 2 baños y gimnasio por menos de 45 mil pesos"
            };
        }

        #region Extractor de Entidades Semánticas

        private static void DetectPropertyType(
            string text,
            List<PropertyTypeVM> types,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            // Sinónimos comunes
            var synonyms = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Apartamento"] = new[] { "apartamento", "apartamentos", "apto", "aptos", "departamento", "departamentos", "penthouse", "estudio", "loft" },
                ["Casa"] = new[] { "casa", "casas", "residencia", "residencias", "townhouse", "vivienda" },
                ["Villa"] = new[] { "villa", "villas", "quinta", "chalet" },
                ["Solar"] = new[] { "solar", "solares", "terreno", "terrenos", "parcela", "lote" },
                ["Comercial"] = new[] { "local", "locales", "oficina", "oficinas", "nave", "comercial", "edificio" }
            };

            foreach (var type in types)
            {
                var normalizedTypeName = NormalizeText(type.Name);
                bool matched = false;

                if (Regex.IsMatch(text, $@"\b{Regex.Escape(normalizedTypeName)}\b"))
                {
                    matched = true;
                }
                else
                {
                    foreach (var kvp in synonyms)
                    {
                        if (normalizedTypeName.Contains(NormalizeText(kvp.Key), StringComparison.OrdinalIgnoreCase))
                        {
                            if (kvp.Value.Any(s => Regex.IsMatch(text, $@"\b{Regex.Escape(s)}\b")))
                            {
                                matched = true;
                                break;
                            }
                        }
                    }
                }

                if (matched)
                {
                    filter.PropertyTypeId = type.Id;
                    entities.PropertyType = type.Name;
                    signals++;
                    break;
                }
            }
        }

        private static void DetectSaleType(
            string text,
            List<SaleTypeVM> saleTypes,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            if (Regex.IsMatch(text, @"\b(alquiler|renta|alquilar|en renta|arrendar)\b"))
            {
                var rentType = saleTypes.FirstOrDefault(s => s.Name.Contains("Alquiler", StringComparison.OrdinalIgnoreCase) || s.Name.Contains("Renta", StringComparison.OrdinalIgnoreCase));
                if (rentType != null)
                {
                    filter.SaleTypeId = rentType.Id;
                    entities.SaleType = rentType.Name;
                    signals++;
                    return;
                }
            }

            if (Regex.IsMatch(text, @"\b(venta|comprar|en venta|adquirir|compra)\b"))
            {
                var saleType = saleTypes.FirstOrDefault(s => s.Name.Contains("Venta", StringComparison.OrdinalIgnoreCase));
                if (saleType != null)
                {
                    filter.SaleTypeId = saleType.Id;
                    entities.SaleType = saleType.Name;
                    signals++;
                }
            }
        }

        private static void DetectRooms(
            string text,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            // Patrón numérico: "3 habitaciones", "3 habs", "3 hab", "3 cuartos", "3 dormitorios"
            var numMatch = Regex.Match(text, @"\b(?:con\s+)?(\d+)\s*(?:\+|\s*o\s*m[aá]s)?\s*(?:habitaci[oó]n(?:es)?|hab(?:s)?|cuartos?|dormitorios?)\b");
            if (numMatch.Success && int.TryParse(numMatch.Groups[1].Value, out int rooms))
            {
                filter.MinRooms = rooms;
                entities.MinRooms = rooms;
                signals++;
                return;
            }

            // Patrón en palabras: "dos habitaciones", "tres cuartos", etc.
            var wordDict = new Dictionary<string, int>
            {
                ["un"] = 1, ["una"] = 1, ["uno"] = 1,
                ["dos"] = 2, ["tres"] = 3, ["cuatro"] = 4, ["cinco"] = 5, ["seis"] = 6
            };

            foreach (var kvp in wordDict)
            {
                if (Regex.IsMatch(text, $@"\b{kvp.Key}\s+(?:habitaci[oó]n(?:es)?|hab(?:s)?|cuartos?|dormitorios?)\b"))
                {
                    filter.MinRooms = kvp.Value;
                    entities.MinRooms = kvp.Value;
                    signals++;
                    return;
                }
            }
        }

        private static void DetectBathrooms(
            string text,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            // Patrón numérico: "2 baños", "2 banos", "1 baño"
            var numMatch = Regex.Match(text, @"\b(?:con\s+)?(\d+)\s*(?:\+|\s*o\s*m[aá]s)?\s*ba[ñn]os?\b");
            if (numMatch.Success && int.TryParse(numMatch.Groups[1].Value, out int bathrooms))
            {
                filter.MinBathrooms = bathrooms;
                entities.MinBathrooms = bathrooms;
                signals++;
                return;
            }

            var wordDict = new Dictionary<string, int>
            {
                ["un"] = 1, ["uno"] = 1, ["dos"] = 2, ["tres"] = 3, ["cuatro"] = 4
            };

            foreach (var kvp in wordDict)
            {
                if (Regex.IsMatch(text, $@"\b{kvp.Key}\s+ba[ñn]os?\b"))
                {
                    filter.MinBathrooms = kvp.Value;
                    entities.MinBathrooms = kvp.Value;
                    signals++;
                    return;
                }
            }
        }

        private static void DetectPrice(
            string text,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            // Rango "entre X y Y"
            var rangeMatch = Regex.Match(text, @"\bentre\s+(?:rd\$|\$)?\s*([0-9.,]+)\s*(millones?|mil|m|k)?\s+y\s+(?:rd\$|\$)?\s*([0-9.,]+)\s*(millones?|mil|m|k)?\b");
            if (rangeMatch.Success)
            {
                decimal? p1 = ParsePriceNumber(rangeMatch.Groups[1].Value, rangeMatch.Groups[2].Value);
                decimal? p2 = ParsePriceNumber(rangeMatch.Groups[3].Value, rangeMatch.Groups[4].Value);
                if (p1.HasValue && p2.HasValue)
                {
                    filter.MinPrice = Math.Min(p1.Value, p2.Value);
                    filter.MaxPrice = Math.Max(p1.Value, p2.Value);
                    entities.MinPrice = filter.MinPrice;
                    entities.MaxPrice = filter.MaxPrice;
                    signals++;
                    return;
                }
            }

            // Precio máximo: "menos de", "hasta", "máximo", "menor a", "por debajo de"
            var maxMatch = Regex.Match(text, @"\b(?:menos\s+de|hasta|m[aá]ximo(?:\s+de)?|por\s+debajo\s+de|menor\s+a)\s+(?:rd\$|\$)?\s*([0-9.,]+)\s*(millones?|mil|m|k)?\b");
            if (maxMatch.Success)
            {
                decimal? max = ParsePriceNumber(maxMatch.Groups[1].Value, maxMatch.Groups[2].Value);
                if (max.HasValue)
                {
                    filter.MaxPrice = max.Value;
                    entities.MaxPrice = max.Value;
                    signals++;
                    return;
                }
            }

            // Precio mínimo: "más de", "desde", "mínimo", "mayor a", "a partir de"
            var minMatch = Regex.Match(text, @"\b(?:m[aá]s\s+de|desde|m[ií]nimo(?:\s+de)?|mayor\s+a|a\s+partir\s+de)\s+(?:rd\$|\$)?\s*([0-9.,]+)\s*(millones?|mil|m|k)?\b");
            if (minMatch.Success)
            {
                decimal? min = ParsePriceNumber(minMatch.Groups[1].Value, minMatch.Groups[2].Value);
                if (min.HasValue)
                {
                    filter.MinPrice = min.Value;
                    entities.MinPrice = min.Value;
                    signals++;
                }
            }
        }

        private static decimal? ParsePriceNumber(string numStr, string? unitStr)
        {
            if (string.IsNullOrWhiteSpace(numStr)) return null;

            numStr = numStr.Trim().Replace(",", ".");
            if (!decimal.TryParse(numStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                return null;

            var unit = unitStr?.Trim().ToLowerInvariant();
            if (unit is "millon" or "millones" or "m")
            {
                return val * 1_000_000m;
            }
            if (unit is "mil" or "k")
            {
                return val * 1_000m;
            }

            // Si el número es pequeño (< 100) y no se especificó unidad, en contexto inmobiliario RD suele ser millones
            if (val is > 0 and <= 100 && string.IsNullOrEmpty(unit))
            {
                return val * 1_000_000m;
            }

            return val;
        }

        private static void DetectLocation(
            string text,
            List<Province> provinces,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            // Buscar en provincias
            foreach (var province in provinces)
            {
                var normProv = NormalizeText(province.Name);
                if (Regex.IsMatch(text, $@"\b{Regex.Escape(normProv)}\b"))
                {
                    filter.ProvinceId = province.Id;
                    entities.Province = province.Name;
                    signals++;
                    break;
                }
            }

            // Sectores dominicanos conocidos
            var commonSectors = new[]
            {
                "Bella Vista", "Piantini", "Naco", "Evaristo Morales", "Gazcue", "Arroyo Hondo",
                "Mirador Sur", "Mirador Norte", "Los Cacicazgos", "Ensanche Paraíso", "Anacaona",
                "Alma Rosa", "Ensanche Ozama", "Gurabo", "Punta Cana", "Bávaro", "Cap Cana",
                "Las Terrenas", "Sosúa", "Cabarete", "Jardines Metropolitanos", "La Julia"
            };

            foreach (var sector in commonSectors)
            {
                var normSector = NormalizeText(sector);
                if (Regex.IsMatch(text, $@"\b{Regex.Escape(normSector)}\b"))
                {
                    filter.Sector = sector;
                    entities.Sector = sector;
                    signals++;
                    break;
                }
            }
        }

        private static void DetectImprovements(
            string text,
            List<ImprovementVM> improvements,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            var matchedIds = new List<int>();

            var synonyms = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Balcón"] = new[] { "balcon", "balcones", "terraza" },
                ["Piscina"] = new[] { "piscina", "alberca", "pileta" },
                ["Gimnasio"] = new[] { "gimnasio", "gym" },
                ["Ascensor"] = new[] { "ascensor", "elevador" },
                ["Planta Eléctrica"] = new[] { "planta", "planta electrica", "generador" },
                ["Seguridad"] = new[] { "seguridad", "vigilancia", "guardian" },
                ["Jacuzzi"] = new[] { "jacuzzi", "hidromasaje" },
                ["Parqueo"] = new[] { "parqueo", "parqueos", "estacionamiento", "garaje", "cochera" },
                ["Patio"] = new[] { "patio", "jardin", "patio trasero" }
            };

            foreach (var imp in improvements)
            {
                var normName = NormalizeText(imp.Name);
                bool matched = false;

                if (Regex.IsMatch(text, $@"\b{Regex.Escape(normName)}\b"))
                {
                    matched = true;
                }
                else
                {
                    foreach (var kvp in synonyms)
                    {
                        if (normName.Contains(NormalizeText(kvp.Key), StringComparison.OrdinalIgnoreCase))
                        {
                            if (kvp.Value.Any(s => Regex.IsMatch(text, $@"\b{Regex.Escape(s)}\b")))
                            {
                                matched = true;
                                break;
                            }
                        }
                    }
                }

                if (matched)
                {
                    matchedIds.Add(imp.Id);
                    entities.Improvements.Add(imp.Name);
                    signals++;
                }
            }

            if (matchedIds.Count > 0)
            {
                filter.ImprovementIds = matchedIds;
            }
        }

        private static void DetectSpecialCriteria(
            string text,
            PropertyFilterViewModel filter,
            AiExtractedEntitiesDto entities,
            ref int signals)
        {
            if (Regex.IsMatch(text, @"\b(virtual|tour\s+virtual|recorrido\s+360|360|matterport)\b"))
            {
                filter.OnlyWithVirtualTour = true;
                entities.HasVirtualTour = true;
                signals++;
            }

            if (Regex.IsMatch(text, @"\b(financiable|con\s+financiamiento|cr[eé]dito|aplica\s+a\s+pr[eé]stamo)\b"))
            {
                filter.OnlyFinanciable = true;
                entities.IsFinanciable = true;
                signals++;
            }

            if (Regex.IsMatch(text, @"\b(destacado|destacados|destacada|premium|top)\b"))
            {
                filter.OnlyFeatured = true;
                entities.IsFeatured = true;
                signals++;
            }
        }

        private static string BuildExplanation(AiExtractedEntitiesDto e, PropertyFilterViewModel f)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(e.PropertyType))
                parts.Add(e.PropertyType);
            else
                parts.Add("Inmuebles");

            if (!string.IsNullOrEmpty(e.SaleType))
                parts.Add($"en {e.SaleType}");

            if (!string.IsNullOrEmpty(e.Sector))
                parts.Add($"en el sector {e.Sector}");
            else if (!string.IsNullOrEmpty(e.Province))
                parts.Add($"en {e.Province}");

            if (e.MinRooms.HasValue)
                parts.Add($"con al menos {e.MinRooms} hab.");

            if (e.MinBathrooms.HasValue)
                parts.Add($"y {e.MinBathrooms} baño(s)");

            if (e.MinPrice.HasValue && e.MaxPrice.HasValue)
                parts.Add($"con precio entre RD$ {e.MinPrice:N0} y RD$ {e.MaxPrice:N0}");
            else if (e.MaxPrice.HasValue)
                parts.Add($"con precio hasta RD$ {e.MaxPrice:N0}");
            else if (e.MinPrice.HasValue)
                parts.Add($"con precio desde RD$ {e.MinPrice:N0}");

            if (e.Improvements.Count > 0)
                parts.Add($"que incluyan {string.Join(", ", e.Improvements)}");

            if (e.HasVirtualTour == true)
                parts.Add("con tour virtual 360°");

            if (parts.Count == 0)
                return "Buscando todas las propiedades disponibles.";

            return "Buscando " + string.Join(" ", parts) + ".";
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        #endregion
    }
}
