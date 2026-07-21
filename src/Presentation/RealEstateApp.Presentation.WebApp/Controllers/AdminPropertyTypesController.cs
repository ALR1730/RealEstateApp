using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.PropertyType;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class AdminPropertyTypesController : Controller
    {
        private readonly IPropertyTypeService _propertyTypeService;

        public AdminPropertyTypesController(IPropertyTypeService propertyTypeService)
        {
            _propertyTypeService = propertyTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _propertyTypeService.GetAllViewModel();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SavePropertyTypeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavePropertyTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _propertyTypeService.Add(vm);
            TempData["SuccessMessage"] = "Tipo de propiedad creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _propertyTypeService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return NotFound();
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavePropertyTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _propertyTypeService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Tipo de propiedad actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _propertyTypeService.Delete(id);
            TempData["SuccessMessage"] = "Tipo de propiedad eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
