using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.DTOs.Property
{
    /// <summary>
    /// DTO de propiedad para exposición en la API REST.
    /// Incluye datos resueltos (nombres de tipo, lista de mejoras e imágenes).
    /// </summary>
    public class PropertyDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Code { get; set; } = string.Empty;

        public string PropertyTypeName { get; set; } = string.Empty;
        public int PropertyTypeId { get; set; }

        public string SaleTypeName { get; set; } = string.Empty;
        public int SaleTypeId { get; set; }

        public int? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public int? MunicipalityId { get; set; }
        public string? MunicipalityName { get; set; }
        public string? Sector { get; set; }
        public string? FullAddress { get; set; }

        public bool IsFeatured { get; set; }
        public System.DateTime? FeaturedUntil { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Rooms { get; set; }

        [Range(0, int.MaxValue)]
        public int Bathrooms { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal SizeInMeters { get; set; }

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Url]
        public string? VideoUrl { get; set; }

        [Url]
        public string? Tour360Url { get; set; }

        public string? MatterportModelId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MontoSeparacion { get; set; }

        [Range(0, 100)]
        public int PorcentajeInicialRequerido { get; set; }

        public bool IsFinanciable { get; set; }

        /// <summary>
        /// URLs de las imágenes de la propiedad.
        /// </summary>
        public List<string> Images { get; set; } = new();

        /// <summary>
        /// Nombres de las mejoras asociadas.
        /// </summary>
        public List<string> Improvements { get; set; } = new();
    }
}
