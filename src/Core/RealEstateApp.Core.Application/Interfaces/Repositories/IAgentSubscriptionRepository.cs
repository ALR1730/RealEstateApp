using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IAgentSubscriptionRepository : IGenericRepository<AgentSubscription>
    {
        Task<AgentSubscription?> GetActiveByAgentIdAsync(string agentId);
    }
}
