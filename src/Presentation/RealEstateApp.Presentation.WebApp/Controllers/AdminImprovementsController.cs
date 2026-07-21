using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Improvement;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class AdminImprovementsController : Controller
    {
        private readonly IImprovementService _improvementService;

        public AdminImprovementsController(IImprovementService improvementService)
        {
            _improvementService = improvementService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _improvementService.GetAllViewModel();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SaveImprovementViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveImprovementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _improvementService.Add(vm);
            TempData["SuccessMessage"] = "Mejora creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _improvementService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return NotFound();
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SaveImprovementViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            await _improvementService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Mejora actualizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _improvementService.Delete(id);
            TempData["SuccessMessage"] = "Mejora eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
