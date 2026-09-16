using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class ProvinceRepository : GenericRepository<Province>, IProvinceRepository
    {
        public ProvinceRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<Province>> GetAllWithMunicipalitiesAsync()
        {
            return await _dbContext.Set<Province>()
                .Include(p => p.Municipalities)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
