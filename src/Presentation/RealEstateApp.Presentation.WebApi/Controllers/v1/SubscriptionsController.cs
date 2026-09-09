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

        /// <summary>
        /// Configura el porcentaje de comisión (%) del plan (administrador).
        /// </summary>
        [HttpPatch("plans/{planId:int}/commission")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePlanCommission(int planId, [FromBody] UpdateCommissionRequest request)
        {
            if (request.Percentage < 0 || request.Percentage > 100)
            {
                return BadRequest(new { hasError = true, error = "El porcentaje de comisión debe estar entre 0 y 100." });
            }

            var updated = await _subscriptionService.UpdatePlanCommissionPercentageAsync(planId, request.Percentage);
            if (!updated)
            {
                return BadRequest(new { hasError = true, error = "Plan no encontrado." });
            }

            return Ok(new { success = true, message = "Porcentaje de comisión actualizado. Se aplica a los nuevos cierres de venta." });
        }

        public class UpdateCommissionRequest
        {
            public decimal Percentage { get; set; }
        }

        // ==================== CRUD de planes (Administrador) ====================

        /// <summary>
        /// Obtiene todos los planes de suscripción (activos e inactivos) para el administrador.
        /// </summary>
        [HttpGet("admin/plans")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPlans()
        {
            return Ok(await _subscriptionService.GetAllPlansAsync());
        }

        /// <summary>
        /// Obtiene un plan de suscripción por id (administrador).
        /// </summary>
        [HttpGet("admin/plans/{planId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlan(int planId)
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null)
            {
                return NotFound(new { hasError = true, error = "Plan no encontrado." });
            }
            return Ok(plan);
        }

        /// <summary>
        /// Crea un nuevo plan de suscripción (administrador).
        /// </summary>
        [HttpPost("admin/plans")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePlan([FromBody] SaveSubscriptionPlanViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || model.MonthlyPrice < 0)
            {
                return BadRequest(new { hasError = true, error = "El plan debe incluir un nombre y un precio mensual válido." });
            }

            var created = await _subscriptionService.CreatePlanAsync(model);
            if (created == null)
            {
                return BadRequest(new { hasError = true, error = "No se pudo crear el plan de suscripción." });
            }

            return Ok(new { success = true, plan = created, message = "Plan de suscripción creado exitosamente." });
        }

        /// <summary>
        /// Actualiza un plan de suscripción existente (administrador).
        /// </summary>
        [HttpPut("admin/plans/{planId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlan(int planId, [FromBody] SaveSubscriptionPlanViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || model.MonthlyPrice < 0)
            {
                return BadRequest(new { hasError = true, error = "El plan debe incluir un nombre y un precio mensual válido." });
            }

            var updated = await _subscriptionService.UpdatePlanAsync(planId, model);
            if (updated == null)
            {
                return NotFound(new { hasError = true, error = "Plan no encontrado." });
            }

            return Ok(new { success = true, plan = updated, message = "Plan de suscripción actualizado exitosamente." });
        }

        /// <summary>
        /// Activa o desactiva un plan de suscripción (administrador).
        /// </summary>
        [HttpPatch("admin/plans/{planId:int}/active")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetPlanActive(int planId, [FromBody] SetPlanActiveRequest request)
        {
            var updated = await _subscriptionService.SetPlanActiveAsync(planId, request.IsActive);
            if (!updated)
            {
                return NotFound(new { hasError = true, error = "Plan no encontrado." });
            }

            return Ok(new { success = true, isActive = request.IsActive, message = request.IsActive ? "Plan activado." : "Plan desactivado." });
        }

        public class SetPlanActiveRequest
        {
            public bool IsActive { get; set; }
        }

        /// <summary>
        /// Elimina un plan de suscripción sin agentes suscritos (administrador).
        /// </summary>
        [HttpDelete("admin/plans/{planId:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeletePlan(int planId)
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null)
            {
                return NotFound(new { hasError = true, error = "Plan no encontrado." });
            }

            var deleted = await _subscriptionService.DeletePlanAsync(planId);
            if (!deleted)
            {
                return Conflict(new { hasError = true, error = "No se puede eliminar un plan con agentes suscritos. Desactívalo en su lugar." });
            }

            return Ok(new { success = true, message = "Plan de suscripción eliminado." });
        }
    }
}
