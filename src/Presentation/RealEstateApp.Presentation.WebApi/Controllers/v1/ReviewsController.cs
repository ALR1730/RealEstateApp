using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Review;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class ReviewsController : BaseApiController
    {
        private readonly IReviewService _reviewService;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(
            IReviewService reviewService,
            UserManager<IdentityUser> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtiene las reseñas públicas de un agente.
        /// </summary>
        [HttpGet("agent/{agentId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAgent(string agentId)
        {
            var reviews = await _reviewService.GetReviewsByAgentAsync(agentId);
            return Ok(reviews);
        }

        /// <summary>
        /// Obtiene el resumen de calificaciones de un agente.
        /// </summary>
        [HttpGet("agent/{agentId}/summary")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary(string agentId)
        {
            var summary = await _reviewService.GetAgentReviewSummaryAsync(agentId);
            return Ok(summary);
        }

        /// <summary>
        /// Indica si el cliente autenticado ya reseñó a este agente por esta propiedad.
        /// </summary>
        [HttpGet("has-reviewed")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> HasReviewed([FromQuery] string agentId, [FromQuery] int propertyId)
        {
            var clientId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var hasReviewed = await _reviewService.HasReviewedAsync(clientId, agentId, propertyId);
            return Ok(hasReviewed);
        }

        /// <summary>
        /// Indica si el cliente autenticado puede reseñar a este agente (transacción completada).
        /// </summary>
        [HttpGet("can-review")]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CanReview([FromQuery] string agentId, [FromQuery] int propertyId)
        {
            var clientId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            var canReview = await _reviewService.CanReviewAsync(clientId, agentId, propertyId);
            return Ok(canReview);
        }

        /// <summary>
        /// Envía una reseña de calificación (1-5) al agente tras completar una transacción.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] SaveAgentReviewViewModel vm)
        {
            if (vm.Rating < 1 || vm.Rating > 5)
            {
                return BadRequest(new { hasError = true, error = "La calificación debe estar entre 1 y 5 estrellas." });
            }

            var clientId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(clientId)) return Unauthorized();

            try
            {
                await _reviewService.SubmitReviewAsync(vm, clientId);
                return Ok(new { success = true, message = "¡Gracias por tu calificación!" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene todas las reseñas del sistema (para el administrador, con identidad del autor).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var reviews = await _reviewService.GetAllAsync();
            return Ok(reviews);
        }
    }
}