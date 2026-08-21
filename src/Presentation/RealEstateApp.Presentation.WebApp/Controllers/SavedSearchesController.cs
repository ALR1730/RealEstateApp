using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.SavedSearch;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize]
    public class SavedSearchesController : Controller
    {
        private readonly ISavedSearchService _savedSearchService;

        public SavedSearchesController(ISavedSearchService savedSearchService)
        {
            _savedSearchService = savedSearchService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var savedSearches = await _savedSearchService.GetUserSavedSearchesAsync(userId);
            return View(savedSearches);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([FromBody] SaveSavedSearchViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Debes iniciar sesión para guardar tu búsqueda." });
            }

            if (string.IsNullOrWhiteSpace(vm.Name))
            {
                return Json(new { success = false, message = "Por favor ingresa un nombre para la búsqueda." });
            }

            vm.UserId = userId;
            var result = await _savedSearchService.SaveSearchAsync(vm);

            return Json(new { success = true, id = result.Id, message = "¡Búsqueda guardada con éxito! Te avisaremos ante nuevas propiedades coincidentes." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAlerts(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "No autorizado." });
            }

            var success = await _savedSearchService.ToggleEmailAlertsAsync(id, userId);
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "No autorizado." });
            }

            var success = await _savedSearchService.DeleteAsync(id, userId);
            return Json(new { success });
        }
    }
}
