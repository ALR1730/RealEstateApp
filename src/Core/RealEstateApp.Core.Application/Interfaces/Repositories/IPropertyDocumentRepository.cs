using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropertyDocumentRepository : IGenericRepository<PropertyDocument>
    {
        Task<List<PropertyDocument>> GetByPropertyIdAsync(int propertyId);
    }
}