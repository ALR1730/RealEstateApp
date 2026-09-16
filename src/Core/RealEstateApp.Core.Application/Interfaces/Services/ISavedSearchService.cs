using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.SavedSearch;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface ISavedSearchService
    {
        Task<List<SavedSearchViewModel>> GetUserSavedSearchesAsync(string userId);
        Task<SavedSearchViewModel?> GetByIdAsync(int id);
        Task<SaveSavedSearchViewModel> SaveSearchAsync(SaveSavedSearchViewModel vm);
        Task<bool> ToggleEmailAlertsAsync(int id, string userId);
        Task<bool> DeleteAsync(int id, string userId);
        Task CheckAndNotifyMatchesAsync(Property newProperty);
    }
}
