using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Subscription;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlanViewModel>> GetAvailablePlansAsync();
        Task<AgentSubscriptionViewModel?> GetCurrentSubscriptionByAgentIdAsync(string agentId);
        Task<AgentSubscriptionDashboardViewModel> GetAgentDashboardViewModelAsync(string agentId);
        Task<bool> SubscribeAgentAsync(string agentId, int planId);
        Task<bool> CanAgentCreatePropertyAsync(string agentId);
        Task<(bool Allowed, string Message)> CanAgentFeaturePropertyAsync(string agentId, int propertyId = 0);
    }
}
