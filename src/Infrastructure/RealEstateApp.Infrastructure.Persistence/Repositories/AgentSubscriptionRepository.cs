using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class AgentSubscriptionRepository : GenericRepository<AgentSubscription>, IAgentSubscriptionRepository
    {
        public AgentSubscriptionRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<AgentSubscription?> GetActiveByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<AgentSubscription>()
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.AgentId == agentId && s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync();
        }
    }
}
