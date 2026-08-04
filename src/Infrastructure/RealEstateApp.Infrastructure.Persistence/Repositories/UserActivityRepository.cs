using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class UserActivityRepository : GenericRepository<UserActivity>, IUserActivityRepository
    {
        public UserActivityRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UserActivity>> GetByUserIdAsync(string userId, int count)
        {
            return await _dbContext.Set<UserActivity>()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
