using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class ProvinceService : IProvinceService
    {
        private readonly IProvinceRepository _provinceRepository;
        private readonly IMunicipalityRepository _municipalityRepository;

        public ProvinceService(
            IProvinceRepository provinceRepository,
            IMunicipalityRepository municipalityRepository)
        {
            _provinceRepository = provinceRepository;
            _municipalityRepository = municipalityRepository;
        }

        public async Task<List<Province>> GetAllWithMunicipalitiesAsync()
        {
            return await _provinceRepository.GetAllWithMunicipalitiesAsync();
        }

        public async Task<List<Municipality>> GetMunicipalitiesByProvinceIdAsync(int provinceId)
        {
            return await _municipalityRepository.GetByProvinceIdAsync(provinceId);
        }
    }
}
