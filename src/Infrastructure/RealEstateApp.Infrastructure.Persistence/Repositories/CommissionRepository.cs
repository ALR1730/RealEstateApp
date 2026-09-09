using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class CommissionRepository : GenericRepository<Commission>, ICommissionRepository
    {
        public CommissionRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<Commission>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<Commission>()
                .Include(c => c.Property)
                .Include(c => c.Offer)
                .Where(c => c.AgentId == agentId)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }

        public async Task<List<Commission>> GetAllWithDetailsAsync()
        {
            return await _dbContext.Set<Commission>()
                .Include(c => c.Property)
                .Include(c => c.Offer)
                .OrderByDescending(c => c.Created)
                .ToListAsync();
        }
    }
}
