using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class FavoritesController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IUserActivityService _userActivityService;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(
            IFavoriteService favoriteService,
            IUserActivityService userActivityService,
            UserManager<IdentityUser> userManager)
        {
            _favoriteService = favoriteService;
            _userActivityService = userActivityService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var favorites = await _favoriteService.GetByClienteId(userId);
            return View(favorites);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int propertyId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var isFav = await _favoriteService.IsFavorite(userId, propertyId);
            if (isFav)
            {
                await _favoriteService.RemoveFavorite(userId, propertyId);
                await _userActivityService.LogActivityAsync(userId, "Removió Favorito", $"Removió la propiedad de sus favoritos", "bi-heartbreak-fill text-danger", $"/Home/Details/{propertyId}");
            }
            else
            {
                await _favoriteService.AddFavorite(userId, propertyId);
                await _userActivityService.LogActivityAsync(userId, "Añadió Favorito", $"Guardó una propiedad en sus favoritos", "bi-heart-fill text-danger", $"/Home/Details/{propertyId}");
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index");
        }
    }
}
