using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IMunicipalityRepository : IGenericRepository<Municipality>
    {
        Task<List<Municipality>> GetByProvinceIdAsync(int provinceId);
    }
}
