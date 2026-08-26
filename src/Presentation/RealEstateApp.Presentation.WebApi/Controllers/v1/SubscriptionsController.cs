using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Subscription;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    public class SubscriptionsController : BaseApiController
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<IdentityUser> _userManager;

        public SubscriptionsController(
            ISubscriptionService subscriptionService,
            UserManager<IdentityUser> userManager)
        {
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtiene todos los planes de suscripción disponibles.
        /// </summary>
        [HttpGet("plans")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlans()
        {
            var plans = await _subscriptionService.GetAvailablePlansAsync();
            return Ok(plans);
        }

        /// <summary>
        /// Obtiene la suscripción actual del agente autenticado.
        /// </summary>
        [HttpGet("my-subscription")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMySubscription()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var current = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(userId);
            return Ok(current);
        }

        /// <summary>
        /// Actualiza / compra un plan de suscripción para el agente.
        /// </summary>
        [HttpPost("upgrade")]
        [Authorize(Roles = "Agent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upgrade([FromBody] UpgradeSubscriptionRequest request)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (request.PlanId <= 0)
            {
                return BadRequest(new { hasError = true, error = "Plan seleccionado inválido." });
            }

            var sub = await _subscriptionService.SubscribeAgentAsync(userId, request.PlanId);
            if (sub == null)
            {
                return BadRequest(new { hasError = true, error = "No se pudo procesar la suscripción al plan." });
            }

            return Ok(new { success = true, subscription = sub, message = "¡Suscripción actualizada exitosamente!" });
        }

        public class UpgradeSubscriptionRequest
        {
            public int PlanId { get; set; }
            public string? PaymentMethodId { get; set; }
        }
    }
}
