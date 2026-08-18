using System.Collections.Generic;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Property : AuditableBaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public decimal SizeInMeters { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public string Status { get; set; } = PropertyStatus.Available;

        // Nuevos campos de mejoras funcionales V2
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? VideoUrl { get; set; }
        public string? Tour360Url { get; set; }
        public string? MatterportModelId { get; set; }
        public decimal MontoSeparacion { get; set; }
        public int PorcentajeInicialRequerido { get; set; }
        public bool IsFinanciable { get; set; } = false;

        // Listados Destacados (Featured)
        public bool IsFeatured { get; set; } = false;
        public System.DateTime? FeaturedUntil { get; set; }

        // Ubicación administrativa en República Dominicana
        public int? ProvinceId { get; set; }
        public Province? Province { get; set; }

        public int? MunicipalityId { get; set; }
        public Municipality? Municipality { get; set; }

        public string? Sector { get; set; }
        public string? FullAddress { get; set; }

        // Relaciones
        public int PropertyTypeId { get; set; }
        public PropertyType? PropertyType { get; set; }

        public int SaleTypeId { get; set; }
        public SaleType? SaleType { get; set; }

        public ICollection<PropertyImage>? Images { get; set; }
        public ICollection<PropertyImprovement>? PropertyImprovements { get; set; }
        public ICollection<Offer>? Offers { get; set; }
        public ICollection<Chat>? Chats { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
        public ICollection<MortgageSimulation>? MortgageSimulations { get; set; }
        public ICollection<PropertyAppointment>? Appointments { get; set; }
    }
}
