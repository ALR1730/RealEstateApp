using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Common;
using RealEstateApp.Core.Application.DTOs.Dashboard;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IPropertyRepository : IGenericRepository<Property>
    {
        Task<PagedResult<Property>> GetPagedAsync(
            int page = PagedResult<Property>.DefaultPage,
            int pageSize = PagedResult<Property>.DefaultPageSize);
        Task<PagedResult<Property>> GetWithFiltersPagedAsync(PropertyFilterViewModel filters, bool includeSold = true);
        Task<List<Property>> GetWithFiltersAsync(PropertyFilterViewModel filters);
        Task<List<Property>> GetByAgentIdAsync(string agentId);
        Task<Property?> GetByCodeAsync(string code);
        Task<List<string>> GetDistinctSectorsAsync(int? provinceId = null, int? municipalityId = null);
        Task<List<Property>> GetCandidateComparablesAsync(int targetPropertyId, int? municipalityId, int? provinceId);
        Task<PropertyDashboardMetricsDto> GetDashboardMetricsAsync();
        Task<string?> GetAgentIdByPropertyIdAsync(int propertyId);
        Task<Property?> FindByIdAsync(int id);
    }
}
