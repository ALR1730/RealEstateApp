using System.Collections.Generic;

namespace RealEstateApp.Core.Application.ViewModels.Property
{
    /// <summary>
    /// ViewModel de lectura para mostrar propiedades en el catálogo de la WebApp MVC.
    /// Incluye datos resueltos de relaciones para evitar consultas adicionales en la vista.
    /// </summary>
    public class PropertyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public decimal SizeInMeters { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Datos resueltos del tipo de propiedad
        public int PropertyTypeId { get; set; }
        public string PropertyTypeName { get; set; } = string.Empty;

        // Datos resueltos del tipo de venta
        public int SaleTypeId { get; set; }
        public string SaleTypeName { get; set; } = string.Empty;

        // Ubicación administrativa en RD
        public int? ProvinceId { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public int? MunicipalityId { get; set; }
        public string MunicipalityName { get; set; } = string.Empty;
        public string? Sector { get; set; }
        public string? FullAddress { get; set; }

        // Datos del agente
        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;

        // Campos multimedia y geolocalización
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? VideoUrl { get; set; }
        public string? Tour360Url { get; set; }
        public string? MatterportModelId { get; set; }

        // Campos financieros
        public decimal MontoSeparacion { get; set; }
        public int PorcentajeInicialRequerido { get; set; }
        public bool IsFinanciable { get; set; }

        // Listados Destacados (Featured)
        public bool IsFeatured { get; set; }
        public System.DateTime? FeaturedUntil { get; set; }
        public bool IsCurrentlyFeatured => IsFeatured && (!FeaturedUntil.HasValue || FeaturedUntil.Value > System.DateTime.UtcNow);

        // Listas resueltas
        public List<string> Images { get; set; } = new();
        public List<string> Improvements { get; set; } = new();

        // Cantidad de favoritos (para estadísticas)
        public int FavoritesCount { get; set; }

        // Indica si el usuario actual la tiene como favorita
        public bool IsFavorite { get; set; }
    }
}
