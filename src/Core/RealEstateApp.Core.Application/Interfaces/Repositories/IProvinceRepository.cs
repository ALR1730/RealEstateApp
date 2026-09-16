using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IProvinceRepository : IGenericRepository<Province>
    {
        Task<List<Province>> GetAllWithMunicipalitiesAsync();
    }
}
