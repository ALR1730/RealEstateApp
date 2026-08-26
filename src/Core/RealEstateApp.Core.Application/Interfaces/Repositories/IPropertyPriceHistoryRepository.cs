using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropertyPriceHistoryRepository : IGenericRepository<PropertyPriceHistory>
    {
        /// <summary>
        /// Obtiene el historial cronológico de cambios de precio de una propiedad.
        /// </summary>
        Task<List<PropertyPriceHistory>> GetByPropertyIdAsync(int propertyId);

        /// <summary>
        /// Obtiene el último cambio de precio registrado para una propiedad.
        /// </summary>
        Task<PropertyPriceHistory?> GetLatestByPropertyIdAsync(int propertyId);
    }
}
