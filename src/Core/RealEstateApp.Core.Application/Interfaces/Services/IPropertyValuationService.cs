using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Valuation;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de Valuación Automatizada (AVM).
    /// Estima el precio de una propiedad basándose en comparables de la misma provincia/sector.
    /// </summary>
    public interface IPropertyValuationService
    {
        /// <summary>
        /// Calcula la valuación estimada de una propiedad usando análisis de comparables.
        /// </summary>
        Task<ValuationResultDto> CalculateValuationAsync(int propertyId, decimal searchRadiusKm = 5.0m);

        /// <summary>
        /// Obtiene la última valuación almacenada de una propiedad.
        /// </summary>
        Task<ValuationResultDto?> GetLastValuationAsync(int propertyId);
    }
}
