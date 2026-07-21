using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropertyImageRepository : IGenericRepository<PropertyImage>
    {
        /// <summary>
        /// Obtiene todas las imágenes de una propiedad.
        /// </summary>
        Task<List<PropertyImage>> GetByPropertyIdAsync(int propertyId);
    }
}
