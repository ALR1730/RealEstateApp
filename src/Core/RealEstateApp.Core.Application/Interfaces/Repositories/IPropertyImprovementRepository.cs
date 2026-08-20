using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropertyImprovementRepository : IGenericRepository<PropertyImprovement>
    {
        Task<List<PropertyImprovement>> GetByPropertyIdAsync(int propertyId);
        Task UpdatePropertyImprovementsAsync(int propertyId, List<int> improvementIds);
    }
}
