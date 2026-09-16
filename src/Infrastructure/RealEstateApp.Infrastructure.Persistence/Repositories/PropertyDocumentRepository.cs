using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class PropertyDocumentRepository : GenericRepository<PropertyDocument>, IPropertyDocumentRepository
    {
        public PropertyDocumentRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<PropertyDocument>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<PropertyDocument>()
                .Include(d => d.Property)
                .Where(d => d.PropertyId == propertyId)
                .OrderByDescending(d => d.Created)
                .ToListAsync();
        }
    }
}