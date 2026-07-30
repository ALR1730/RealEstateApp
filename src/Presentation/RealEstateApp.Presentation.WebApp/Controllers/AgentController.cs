using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;
        private readonly IOfferService _offerService;
        private readonly UserManager<IdentityUser> _userManager;

        public AgentController(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IOfferService offerService,
            UserManager<IdentityUser> userManager)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _offerService = offerService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Properties()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var properties = await _propertyService.GetByAgentId(userId);
            return View(properties);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProperty()
        {
            var vm = new SavePropertyViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(SavePropertyViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            vm.AgentId = userId;

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            await _propertyService.Add(vm);
            TempData["SuccessMessage"] = "Propiedad creada exitosamente.";
            return RedirectToAction(nameof(Properties));
        }

        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var vm = await _propertyService.GetByIdSaveViewModel(id);
            if (vm == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (vm.AgentId != userId)
            {
                return RedirectToAction(nameof(Properties));
            }

            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(SavePropertyViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var existing = await _propertyService.GetByIdSaveViewModel(vm.Id);
            if (existing == null || existing.AgentId != userId)
            {
                return RedirectToAction(nameof(Properties));
            }

            vm.AgentId = userId;

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            await _propertyService.Update(vm, vm.Id);
            TempData["SuccessMessage"] = "Propiedad actualizada exitosamente.";
            return RedirectToAction(nameof(Properties));
        }

        private async Task PopulateDropdowns(SavePropertyViewModel vm)
        {
            var propTypes = await _propertyTypeService.GetAllViewModel();
            vm.PropertyTypes = propTypes.Select(pt => new RealEstateApp.Core.Application.ViewModels.Property.PropertyTypeViewModel { Id = pt.Id, Name = pt.Name }).ToList();

            var saleTypes = await _saleTypeService.GetAllViewModel();
            vm.SaleTypes = saleTypes.Select(st => new RealEstateApp.Core.Application.ViewModels.Property.SaleTypeViewModel { Id = st.Id, Name = st.Name }).ToList();

            var imps = await _improvementService.GetAllViewModel();
            vm.Improvements = imps.Select(i => new RealEstateApp.Core.Application.ViewModels.Property.ImprovementViewModel { Id = i.Id, Name = i.Name }).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var existing = await _propertyService.GetByIdSaveViewModel(id);
            if (existing == null || existing.AgentId != userId)
            {
                TempData["ErrorMessage"] = "No tiene permisos para eliminar esta propiedad.";
                return RedirectToAction(nameof(Properties));
            }

            await _propertyService.Delete(id);
            TempData["SuccessMessage"] = "Propiedad eliminada correctamente.";
            return RedirectToAction(nameof(Properties));
        }

        public async Task<IActionResult> Offers()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener propiedades del agente y sus ofertas filtradas directamente en la BD
            var agentProperties = await _propertyService.GetByAgentId(userId);
            var agentPropertyIds = agentProperties.Select(p => p.Id).ToList();
            var agentOffers = await _offerService.GetByPropertyIds(agentPropertyIds);

            return View(agentOffers);
        }

        /// <summary>
        /// Regla de Negocio Atómica:
        /// Al Aceptar una oferta → Propiedad pasa a "Vendida" → Rechazo en cascada de ofertas pendientes.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOffer(int offerId)
        {
            try
            {
                await _offerService.AcceptOffer(offerId);
                TempData["SuccessMessage"] = "Oferta aceptada exitosamente. La propiedad cambió a estado 'Vendida' y las demás ofertas competidoras fueron rechazadas en cascada.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Offers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOffer(int offerId)
        {
            try
            {
                await _offerService.RejectOffer(offerId);
                TempData["SuccessMessage"] = "Oferta rechazada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Offers));
        }
    }
}
