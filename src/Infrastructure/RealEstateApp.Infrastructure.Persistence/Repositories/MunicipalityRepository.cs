using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class MunicipalityRepository : GenericRepository<Municipality>, IMunicipalityRepository
    {
        public MunicipalityRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<Municipality>> GetByProvinceIdAsync(int provinceId)
        {
            return await _dbContext.Set<Municipality>()
                .Where(m => m.ProvinceId == provinceId)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }
    }
}
