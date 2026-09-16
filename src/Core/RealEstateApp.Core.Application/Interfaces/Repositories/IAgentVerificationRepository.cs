using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IAgentVerificationRepository : IGenericRepository<AgentVerification>
    {
        Task<AgentVerification?> GetByAgentIdAsync(string agentId);
        Task<List<AgentVerification>> GetPendingVerificationsAsync();
    }
}
