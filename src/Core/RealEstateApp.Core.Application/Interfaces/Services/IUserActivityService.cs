using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.UserActivity;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IUserActivityService
    {
        Task LogActivityAsync(string userId, string action, string description, string icon = "bi-activity", string? targetUrl = null);
        Task<List<UserActivityViewModel>> GetRecentActivitiesAsync(string userId, int count = 20);
    }
}
