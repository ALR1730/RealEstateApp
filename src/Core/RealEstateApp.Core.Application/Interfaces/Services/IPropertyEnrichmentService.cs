using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de servicio especializado en enriquecimiento de modelos de propiedades
    /// con cálculos multimoneda, nombres de agentes y detección de rebajas de precios.
    /// </summary>
    public interface IPropertyEnrichmentService
    {
        Task EnrichPropertiesWithCurrencyAsync(List<PropertyViewModel> viewModels);
        Task EnrichAgentNamesAsync(List<PropertyViewModel> viewModels);
        Task EnrichPriceDropAsync(PropertyViewModel viewModel);
    }
}
