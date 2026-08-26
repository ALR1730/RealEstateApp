using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyPriceHistoryRepository : GenericRepository<PropertyPriceHistory>, IPropertyPriceHistoryRepository
    {
        public PropertyPriceHistoryRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<PropertyPriceHistory>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.PropertyPriceHistories
                .Where(ph => ph.PropertyId == propertyId)
                .OrderBy(ph => ph.ChangeDate)
                .ToListAsync();
        }

        public async Task<PropertyPriceHistory?> GetLatestByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.PropertyPriceHistories
                .Where(ph => ph.PropertyId == propertyId)
                .OrderByDescending(ph => ph.ChangeDate)
                .FirstOrDefaultAsync();
        }
    }
}
