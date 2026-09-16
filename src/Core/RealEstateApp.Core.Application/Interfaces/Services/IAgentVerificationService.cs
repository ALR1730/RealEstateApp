using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Agent;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IAgentVerificationService
    {
        Task<AgentVerificationViewModel?> GetByAgentIdAsync(string agentId);
        Task<List<AgentVerificationViewModel>> GetAllAsync();
        Task<List<AgentVerificationViewModel>> GetPendingAsync();
        Task<bool> SubmitVerificationAsync(AgentVerificationViewModel vm);
        Task<bool> ReviewVerificationAsync(int id, bool approved, string? rejectionReason, string adminId);
        Task<bool> IsAgentVerifiedAsync(string agentId);
    }
}
