using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class AgentVerificationRepository : GenericRepository<AgentVerification>, IAgentVerificationRepository
    {
        public AgentVerificationRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<AgentVerification?> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<AgentVerification>()
                .FirstOrDefaultAsync(v => v.AgentId == agentId);
        }

        public async Task<List<AgentVerification>> GetPendingVerificationsAsync()
        {
            return await _dbContext.Set<AgentVerification>()
                .Where(v => v.Status == VerificationStatus.Pending)
                .OrderBy(v => v.Created)
                .ToListAsync();
        }
    }
}
