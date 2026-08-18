using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerController : Controller
    {
        private const int MaxOwnerProperties = 2;

        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;
        private readonly IProvinceRepository _provinceRepository;
        private readonly IMunicipalityRepository _municipalityRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public OwnerController(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IProvinceRepository provinceRepository,
            IMunicipalityRepository municipalityRepository,
            UserManager<IdentityUser> userManager)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _provinceRepository = provinceRepository;
            _municipalityRepository = municipalityRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var properties = await _propertyService.GetByAgentId(userId);
            ViewBag.MaxOwnerProperties = MaxOwnerProperties;
            ViewBag.CanCreate = properties.Count < MaxOwnerProperties;

            return View(properties);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProperty()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var properties = await _propertyService.GetByAgentId(userId);
            if (properties.Count >= MaxOwnerProperties)
            {
                TempData["ErrorMessage"] = $"Como propietario directo puedes publicar un máximo de {MaxOwnerProperties} inmuebles simultáneos.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new SavePropertyViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(SavePropertyViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var properties = await _propertyService.GetByAgentId(userId);
            if (properties.Count >= MaxOwnerProperties)
            {
                TempData["ErrorMessage"] = $"Como propietario directo puedes publicar un máximo de {MaxOwnerProperties} inmuebles simultáneos.";
                return RedirectToAction(nameof(Index));
            }

            vm.AgentId = userId;

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            await _propertyService.Add(vm);
            TempData["SuccessMessage"] = "Tu inmueble ha sido publicado exitosamente para venta/alquiler directo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var vm = await _propertyService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (vm.AgentId != userId)
            {
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(int id, SavePropertyViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var existing = await _propertyService.GetByIdSaveViewModel(id);
            if (existing == null || existing.AgentId != userId)
            {
                return RedirectToAction(nameof(Index));
            }

            vm.AgentId = userId;

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            await _propertyService.Update(vm, id);
            TempData["SuccessMessage"] = "Tu inmueble ha sido actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var existing = await _propertyService.GetByIdSaveViewModel(id);
            if (existing == null || existing.AgentId != userId)
            {
                return RedirectToAction(nameof(Index));
            }

            await _propertyService.Delete(id);
            TempData["SuccessMessage"] = "El inmueble ha sido eliminado de tu catálogo de propietario.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(SavePropertyViewModel vm)
        {
            var types = await _propertyTypeService.GetAllViewModel();
            var sales = await _saleTypeService.GetAllViewModel();
            var improvements = await _improvementService.GetAllViewModel();
            var provinces = await _provinceRepository.GetAllAsync();

            vm.PropertyTypes = types.Select(t => new RealEstateApp.Core.Application.ViewModels.Property.PropertyTypeViewModel { Id = t.Id, Name = t.Name }).ToList();
            vm.SaleTypes = sales.Select(s => new RealEstateApp.Core.Application.ViewModels.Property.SaleTypeViewModel { Id = s.Id, Name = s.Name }).ToList();
            vm.Improvements = improvements.Select(i => new RealEstateApp.Core.Application.ViewModels.Property.ImprovementViewModel { Id = i.Id, Name = i.Name }).ToList();
            vm.Provinces = provinces.Select(p => new ProvinceDropdownViewModel { Id = p.Id, Name = p.Name, IsoCode = p.IsoCode }).ToList();

            if (vm.ProvinceId.HasValue)
            {
                var municipalities = await _municipalityRepository.GetByProvinceIdAsync(vm.ProvinceId.Value);
                vm.Municipalities = municipalities.Select(m => new MunicipalityDropdownViewModel { Id = m.Id, Name = m.Name, ProvinceId = m.ProvinceId }).ToList();
            }
        }
    }
}
