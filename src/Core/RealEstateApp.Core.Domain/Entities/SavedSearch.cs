using System;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class SavedSearch : AuditableBaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Criterios de búsqueda
        public int? PropertyTypeId { get; set; }
        public PropertyType? PropertyType { get; set; }

        public int? SaleTypeId { get; set; }
        public SaleType? SaleType { get; set; }

        public int? ProvinceId { get; set; }
        public Province? Province { get; set; }

        public int? MunicipalityId { get; set; }
        public Municipality? Municipality { get; set; }

        public string? Sector { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinRooms { get; set; }
        public int? MaxRooms { get; set; }
        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }
        public decimal? MinSizeInMeters { get; set; }
        public decimal? MaxSizeInMeters { get; set; }
        public bool? OnlyFinanciable { get; set; }
        public bool? OnlyWithVirtualTour { get; set; }

        // Configuración de alertas
        public bool EmailAlertsEnabled { get; set; } = true;
        public bool InAppAlertsEnabled { get; set; } = true;
        public DateTime? LastAlertSent { get; set; }
    }
}
