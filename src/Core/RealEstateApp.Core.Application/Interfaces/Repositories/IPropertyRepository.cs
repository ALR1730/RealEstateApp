using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropertyRepository : IGenericRepository<Property>
    {
        Task<List<Property>> GetWithFiltersAsync(PropertyFilterViewModel filters);
        Task<List<Property>> GetByAgentIdAsync(string agentId);
        Task<Property?> GetByCodeAsync(string code);
        Task<List<string>> GetDistinctSectorsAsync(int? provinceId = null, int? municipalityId = null);
    }
}
