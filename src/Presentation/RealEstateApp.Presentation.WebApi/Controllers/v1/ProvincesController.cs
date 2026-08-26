using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class ProvincesController : BaseApiController
    {
        private readonly IProvinceService _provinceService;

        public ProvincesController(IProvinceService provinceService)
        {
            _provinceService = provinceService;
        }

        /// <summary>
        /// Obtiene todas las provincias de República Dominicana con sus municipios.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var provinces = await _provinceService.GetAllWithMunicipalitiesAsync();
            return Ok(provinces);
        }

        /// <summary>
        /// Obtiene los municipios de una provincia específica.
        /// </summary>
        [HttpGet("{provinceId}/municipalities")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMunicipalities(int provinceId)
        {
            var municipalities = await _provinceService.GetMunicipalitiesByProvinceIdAsync(provinceId);
            return Ok(municipalities);
        }
    }
}
