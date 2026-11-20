using System.Collections.Generic;
using RealEstateApp.Core.Domain.Entities;
using ImprovementVM = RealEstateApp.Core.Application.ViewModels.Improvement.ImprovementViewModel;
using PropertyTypeVM = RealEstateApp.Core.Application.ViewModels.PropertyType.PropertyTypeViewModel;
using SaleTypeVM = RealEstateApp.Core.Application.ViewModels.SaleType.SaleTypeViewModel;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class AiSearchCatalogSnapshot
    {
        public AiSearchCatalogSnapshot(
            IReadOnlyList<PropertyTypeVM> propertyTypes,
            IReadOnlyList<SaleTypeVM> saleTypes,
            IReadOnlyList<ImprovementVM> improvements,
            IReadOnlyList<Province> provinces)
        {
            PropertyTypes = propertyTypes;
            SaleTypes = saleTypes;
            Improvements = improvements;
            Provinces = provinces;
        }

        public IReadOnlyList<PropertyTypeVM> PropertyTypes { get; }
        public IReadOnlyList<SaleTypeVM> SaleTypes { get; }
        public IReadOnlyList<ImprovementVM> Improvements { get; }
        public IReadOnlyList<Province> Provinces { get; }
    }
}
