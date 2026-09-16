using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Core.Application.DTOs.AiSearch
{
    /// <summary>
    /// Solicitud de búsqueda en lenguaje natural.
    /// </summary>
    public class AiSearchQueryRequest
    {
        [Required(ErrorMessage = "La consulta de búsqueda es requerida")]
        [MinLength(2, ErrorMessage = "La consulta debe tener al menos 2 caracteres")]
        public string Query { get; set; } = string.Empty;
    }

    /// <summary>
    /// Entidades clave extraídas de la consulta en lenguaje natural.
    /// </summary>
    public class AiExtractedEntitiesDto
    {
        public string? PropertyType { get; set; }
        public string? SaleType { get; set; }
        public string? Province { get; set; }
        public string? Sector { get; set; }
        public int? MinRooms { get; set; }
        public int? MinBathrooms { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<string> Improvements { get; set; } = new();
        public bool? HasVirtualTour { get; set; }
        public bool? IsFinanciable { get; set; }
        public bool? IsFeatured { get; set; }
    }

    /// <summary>
    /// Resultado de la interpretación semántica y búsqueda de IA.
    /// </summary>
    public class AiSearchInterpretationDto
    {
        public string OriginalQuery { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public AiExtractedEntitiesDto ExtractedEntities { get; set; } = new();
        public PropertyFilterViewModel ParsedFilter { get; set; } = new();
        public int MatchedPropertiesCount { get; set; }
        public List<PropertyDto> Properties { get; set; } = new();
    }

    /// <summary>
    /// Sugerencias rápidas de búsqueda para inspirar al usuario.
    /// </summary>
    public class AiSearchSuggestionsDto
    {
        public List<string> Suggestions { get; set; } = new();
    }
}
