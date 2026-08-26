using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Favorite;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Client")]
    public class FavoritesController : BaseApiController
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Obtiene el catálogo de favoritos del cliente autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<FavoriteViewModel>))]
        public async Task<IActionResult> GetAsync()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var favorites = await _favoriteService.GetByClienteId(clientId);
            return Ok(favorites);
        }

        /// <summary>
        /// Comprueba si una propiedad está marcada como favorita.
        /// </summary>
        [HttpGet("check/{propertyId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<IActionResult> IsFavoriteAsync(int propertyId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var isFav = await _favoriteService.IsFavorite(clientId, propertyId);
            return Ok(isFav);
        }

        /// <summary>
        /// Agrega una propiedad al catálogo de favoritos del cliente.
        /// </summary>
        [HttpPost("{propertyId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AddFavoriteAsync(int propertyId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            await _favoriteService.AddFavorite(clientId, propertyId);
            return Ok(new { success = true, message = "Propiedad agregada a favoritos." });
        }

        /// <summary>
        /// Remueve una propiedad de los favoritos del cliente.
        /// </summary>
        [HttpDelete("{propertyId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveFavoriteAsync(int propertyId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            await _favoriteService.RemoveFavorite(clientId, propertyId);
            return Ok(new { success = true, message = "Propiedad removida de favoritos." });
        }
    }
}
