using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropertyValuationRepository : IGenericRepository<PropertyValuation>
    {
        Task<PropertyValuation?> GetByPropertyIdAsync(int propertyId);
    }
}
