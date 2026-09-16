using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.SavedSearch;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Client")]
    public class SavedSearchesController : BaseApiController
    {
        private readonly ISavedSearchService _savedSearchService;

        public SavedSearchesController(ISavedSearchService savedSearchService)
        {
            _savedSearchService = savedSearchService;
        }

        /// <summary>
        /// Obtiene todas las búsquedas guardadas del cliente autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SavedSearchViewModel>))]
        public async Task<IActionResult> GetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var searches = await _savedSearchService.GetUserSavedSearchesAsync(userId);
            return Ok(searches);
        }

        /// <summary>
        /// Guarda un nuevo criterio de búsqueda con soporte de alertas.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SaveSavedSearchViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveAsync([FromBody] SaveSavedSearchViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            model.UserId = userId;
            var created = await _savedSearchService.SaveSearchAsync(model);
            return Ok(created);
        }

        /// <summary>
        /// Alterna las alertas por correo para una búsqueda guardada.
        /// </summary>
        [HttpPatch("{id:int}/toggle-alerts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleAlertsAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _savedSearchService.ToggleEmailAlertsAsync(id, userId);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Elimina una búsqueda guardada.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _savedSearchService.DeleteAsync(id, userId);
            return Ok(new { success = result });
        }
    }
}
