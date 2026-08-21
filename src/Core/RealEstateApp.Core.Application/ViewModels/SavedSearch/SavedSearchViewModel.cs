using System;

namespace RealEstateApp.Core.Application.ViewModels.SavedSearch
{
    public class SavedSearchViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Criterios de búsqueda
        public int? PropertyTypeId { get; set; }
        public string? PropertyTypeName { get; set; }

        public int? SaleTypeId { get; set; }
        public string? SaleTypeName { get; set; }

        public int? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }

        public int? MunicipalityId { get; set; }
        public string? MunicipalityName { get; set; }

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

        // Alertas
        public bool EmailAlertsEnabled { get; set; } = true;
        public bool InAppAlertsEnabled { get; set; } = true;
        public DateTime? LastAlertSent { get; set; }
        public DateTime Created { get; set; }

        // Criterios formateados en texto amigable para la interfaz
        public string SummaryCriteria { get; set; } = string.Empty;
        
        // Cantidad de propiedades actualmente coincidentes en el catálogo
        public int CurrentMatchingCount { get; set; }
    }
}
