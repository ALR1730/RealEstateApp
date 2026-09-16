using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class LeadPipelineRepository : GenericRepository<LeadPipeline>, ILeadPipelineRepository
    {
        public LeadPipelineRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<LeadPipeline>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.LeadPipelines
                .Include(lp => lp.Property)
                .Where(lp => lp.AgentId == agentId)
                .OrderBy(lp => lp.Stage)
                .ThenBy(lp => lp.SortOrder)
                .ToListAsync();
        }

        public async Task<List<LeadPipeline>> GetByAgentAndStageAsync(string agentId, string stage)
        {
            return await _dbContext.LeadPipelines
                .Include(lp => lp.Property)
                .Where(lp => lp.AgentId == agentId && lp.Stage == stage)
                .OrderBy(lp => lp.SortOrder)
                .ToListAsync();
        }

        public async Task<LeadPipeline?> GetByIdWithPropertyAsync(int id)
        {
            return await _dbContext.LeadPipelines
                .Include(lp => lp.Property)
                .FirstOrDefaultAsync(lp => lp.Id == id);
        }
    }
}
