using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.SaleType;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class AdminSaleTypesController : Controller
    {
        private readonly ISaleTypeService _saleTypeService;

        public AdminSaleTypesController(ISaleTypeService saleTypeService)
        {
            _saleTypeService = saleTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _saleTypeService.GetAllViewModel();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SaveSaleTypeViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveSaleTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _saleTypeService.Add(vm);
            TempData["SuccessMessage"] = "Tipo de venta creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _saleTypeService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return NotFound();
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SaveSaleTypeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _saleTypeService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Tipo de venta actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _saleTypeService.Delete(id);
            TempData["SuccessMessage"] = "Tipo de venta eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
