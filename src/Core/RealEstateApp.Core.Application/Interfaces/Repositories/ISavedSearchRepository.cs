using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface ISavedSearchRepository : IGenericRepository<SavedSearch>
    {
        Task<List<SavedSearch>> GetByUserIdAsync(string userId);
        Task<List<SavedSearch>> GetMatchingSearchesAsync(Property property);
    }
}
