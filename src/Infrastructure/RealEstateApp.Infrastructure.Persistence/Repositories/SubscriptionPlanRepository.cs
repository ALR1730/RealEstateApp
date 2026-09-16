using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<SubscriptionPlan>> GetActivePlansAsync()
        {
            return await _dbContext.Set<SubscriptionPlan>()
                .Where(p => p.IsActive)
                .OrderBy(p => p.MonthlyPrice)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetDefaultFreePlanAsync()
        {
            return await _dbContext.Set<SubscriptionPlan>()
                .FirstOrDefaultAsync(p => p.MonthlyPrice == 0 && p.IsActive);
        }
    }
}
