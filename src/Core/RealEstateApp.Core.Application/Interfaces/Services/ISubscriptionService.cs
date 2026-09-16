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

        /// <summary>
        /// Configura el porcentaje de comisión (0-100) de un plan (administrador).
        /// </summary>
        Task<bool> UpdatePlanCommissionPercentageAsync(int planId, decimal percentage);

        // ==================== Administración de planes (CRUD) ====================
        Task<List<SubscriptionPlanViewModel>> GetAllPlansAsync();
        Task<SubscriptionPlanViewModel?> GetPlanByIdAsync(int planId);
        Task<SubscriptionPlanViewModel?> CreatePlanAsync(SaveSubscriptionPlanViewModel model);
        Task<SubscriptionPlanViewModel?> UpdatePlanAsync(int planId, SaveSubscriptionPlanViewModel model);
        Task<bool> SetPlanActiveAsync(int planId, bool isActive);
        Task<bool> DeletePlanAsync(int planId);
    }
}
