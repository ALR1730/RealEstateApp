using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyImageRepository : GenericRepository<PropertyImage>, IPropertyImageRepository
    {
        public PropertyImageRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<PropertyImage>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<PropertyImage>()
                .Where(pi => pi.PropertyId == propertyId)
                .ToListAsync();
        }
    }
}
