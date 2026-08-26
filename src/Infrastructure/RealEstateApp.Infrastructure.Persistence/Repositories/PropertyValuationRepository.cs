using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyValuationRepository : GenericRepository<PropertyValuation>, IPropertyValuationRepository
    {
        public PropertyValuationRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<PropertyValuation?> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.PropertyValuations
                .FirstOrDefaultAsync(v => v.PropertyId == propertyId);
        }
    }
}
