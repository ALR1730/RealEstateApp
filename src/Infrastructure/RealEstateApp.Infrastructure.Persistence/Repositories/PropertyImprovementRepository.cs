using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyImprovementRepository : GenericRepository<PropertyImprovement>, IPropertyImprovementRepository
    {
        public PropertyImprovementRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<PropertyImprovement>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.PropertyImprovements
                .AsNoTracking()
                .Include(pi => pi.Improvement)
                .Where(pi => pi.PropertyId == propertyId)
                .ToListAsync();
        }

        public async Task UpdatePropertyImprovementsAsync(int propertyId, List<int> improvementIds)
        {
            var existing = await _dbContext.PropertyImprovements
                .Where(pi => pi.PropertyId == propertyId)
                .ToListAsync();

            if (existing.Any())
            {
                _dbContext.PropertyImprovements.RemoveRange(existing);
            }

            if (improvementIds != null && improvementIds.Any())
            {
                var newRecords = improvementIds.Select(id => new PropertyImprovement
                {
                    PropertyId = propertyId,
                    ImprovementId = id
                });
                await _dbContext.PropertyImprovements.AddRangeAsync(newRecords);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
