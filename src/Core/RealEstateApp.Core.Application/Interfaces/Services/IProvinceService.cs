using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IProvinceService
    {
        Task<List<Province>> GetAllWithMunicipalitiesAsync();
        Task<List<Municipality>> GetMunicipalitiesByProvinceIdAsync(int provinceId);
    }
}
