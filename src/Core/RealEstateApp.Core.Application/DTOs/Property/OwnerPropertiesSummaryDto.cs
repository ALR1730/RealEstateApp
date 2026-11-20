using System.Collections.Generic;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Application.DTOs.Property
{
    public class OwnerPropertiesSummaryDto
    {
        public int MaxAllowed { get; set; } = PropertyLimits.MaxOwnerProperties;
        public int CurrentCount { get; set; }
        public bool CanCreate { get; set; }
        public List<PropertyViewModel> Properties { get; set; } = new();
    }
}
