using System.Collections.Generic;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Core.Application.ViewModels.Agent
{
    public class AgentPropertyFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int? PropertyTypeId { get; set; }
        public int? SaleTypeId { get; set; }
        public bool OnlyFeatured { get; set; }

        public List<PropertyTypeViewModel> PropertyTypes { get; set; } = new();
        public List<SaleTypeViewModel> SaleTypes { get; set; } = new();

        public List<PropertyViewModel> Properties { get; set; } = new();

        // Métricas de inventario para el dashboard del agente
        public int TotalPropertiesCount { get; set; }
        public int AvailableCount { get; set; }
        public int ReservedCount { get; set; }
        public int SoldCount { get; set; }
        public int FeaturedCount { get; set; }
    }
}
