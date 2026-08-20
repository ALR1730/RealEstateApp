using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Application.ViewModels.Subscription;

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
        private readonly IProvinceRepository _provinceRepository;
        private readonly IMunicipalityRepository _municipalityRepository;
        private readonly IAgentVerificationService _verificationService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<IdentityUser> _userManager;

        public AgentController(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IOfferService offerService,
            IProvinceRepository provinceRepository,
            IMunicipalityRepository municipalityRepository,
            IAgentVerificationService verificationService,
            ISubscriptionService subscriptionService,
            UserManager<IdentityUser> userManager)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _offerService = offerService;
            _provinceRepository = provinceRepository;
            _municipalityRepository = municipalityRepository;
            _verificationService = verificationService;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Properties(AgentPropertyFilterViewModel filter)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var allProperties = await _propertyService.GetByAgentId(userId);
            var currentSub = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(userId);

            // Calcular estadísticas
            filter.TotalPropertiesCount = allProperties.Count;
            filter.AvailableCount = allProperties.Count(p => p.Status == RealEstateApp.Core.Domain.Constants.PropertyStatus.Available);
            filter.ReservedCount = allProperties.Count(p => p.Status == RealEstateApp.Core.Domain.Constants.PropertyStatus.Reserved);
            filter.SoldCount = allProperties.Count(p => p.Status == RealEstateApp.Core.Domain.Constants.PropertyStatus.Sold);
            filter.FeaturedCount = allProperties.Count(p => p.IsCurrentlyFeatured);
            filter.MaxFeaturedAllowed = currentSub?.MaxFeaturedProperties ?? 0;
            filter.PlanName = currentSub?.PlanName ?? "Gratuito";

            // Aplicar filtros
            var filtered = allProperties.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                filtered = filtered.Where(p => 
                    (p.Name != null && p.Name.ToLower().Contains(term)) ||
                    (p.Code != null && p.Code.ToLower().Contains(term)) ||
                    (p.Sector != null && p.Sector.ToLower().Contains(term)) ||
                    (p.FullAddress != null && p.FullAddress.ToLower().Contains(term))
                );
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                filtered = filtered.Where(p => string.Equals(p.Status, filter.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.PropertyTypeId.HasValue && filter.PropertyTypeId.Value > 0)
            {
                filtered = filtered.Where(p => p.PropertyTypeId == filter.PropertyTypeId.Value);
            }

            if (filter.SaleTypeId.HasValue && filter.SaleTypeId.Value > 0)
            {
                filtered = filtered.Where(p => p.SaleTypeId == filter.SaleTypeId.Value);
            }

            if (filter.OnlyFeatured)
            {
                filtered = filtered.Where(p => p.IsCurrentlyFeatured);
            }

            filter.Properties = filtered.ToList();

            // Cargar dropdowns
            var propTypes = await _propertyTypeService.GetAllViewModel();
            filter.PropertyTypes = propTypes.Select(pt => new RealEstateApp.Core.Application.ViewModels.Property.PropertyTypeViewModel { Id = pt.Id, Name = pt.Name }).ToList();

            var saleTypes = await _saleTypeService.GetAllViewModel();
            filter.SaleTypes = saleTypes.Select(st => new RealEstateApp.Core.Application.ViewModels.Property.SaleTypeViewModel { Id = st.Id, Name = st.Name }).ToList();

            return View(filter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var property = await _propertyService.GetByIdSaveViewModel(id);
            if (property == null || property.AgentId != userId)
            {
                TempData["ErrorMessage"] = "No tienes permisos para modificar esta propiedad.";
                return RedirectToAction(nameof(Properties));
            }

            if (property.IsFeatured)
            {
                await _propertyService.ToggleFeaturedAsync(id, 30);
                TempData["SuccessMessage"] = $"El inmueble '{property.Name}' ha sido retirado de la sección de Destacados.";
            }
            else
            {
                var validation = await _subscriptionService.CanAgentFeaturePropertyAsync(userId, id);
                if (!validation.Allowed)
                {
                    TempData["ErrorMessage"] = validation.Message;
                    return RedirectToAction(nameof(Properties));
                }

                await _propertyService.ToggleFeaturedAsync(id, 30);
                TempData["SuccessMessage"] = $"¡El inmueble '{property.Name}' ahora está DESTACADO en el catálogo principal! ⭐";
            }

            return RedirectToAction(nameof(Properties));
        }

        [HttpGet]
        public async Task<IActionResult> CreateProperty()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var canCreate = await _subscriptionService.CanAgentCreatePropertyAsync(userId);
            if (!canCreate)
            {
                TempData["ErrorMessage"] = "Has alcanzado el límite máximo de propiedades permitidas por tu plan de suscripción actual. Actualiza tu plan para continuar publicando.";
                return RedirectToAction(nameof(Subscriptions));
            }

            var canFeature = await _subscriptionService.CanAgentFeaturePropertyAsync(userId, 0);
            var currentSub = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(userId);
            ViewBag.CanFeature = canFeature.Allowed;
            ViewBag.MaxFeatured = currentSub?.MaxFeaturedProperties ?? 0;
            ViewBag.PlanName = currentSub?.PlanName ?? "Gratuito";
            ViewBag.FeatureMessage = canFeature.Message;

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

            var canCreate = await _subscriptionService.CanAgentCreatePropertyAsync(userId);
            if (!canCreate)
            {
                TempData["ErrorMessage"] = "Has alcanzado el límite máximo de propiedades permitidas por tu plan de suscripción actual. Actualiza tu plan para continuar publicando.";
                return RedirectToAction(nameof(Subscriptions));
            }

            if (vm.IsFeatured)
            {
                var featureCheck = await _subscriptionService.CanAgentFeaturePropertyAsync(userId, 0);
                if (!featureCheck.Allowed)
                {
                    ModelState.AddModelError("IsFeatured", featureCheck.Message);
                    vm.IsFeatured = false;
                }
            }

            vm.AgentId = userId;

            if (!ModelState.IsValid)
            {
                var canFeature = await _subscriptionService.CanAgentFeaturePropertyAsync(userId, 0);
                var currentSub = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(userId);
                ViewBag.CanFeature = canFeature.Allowed;
                ViewBag.MaxFeatured = currentSub?.MaxFeaturedProperties ?? 0;
                ViewBag.PlanName = currentSub?.PlanName ?? "Gratuito";
                ViewBag.FeatureMessage = canFeature.Message;

                await PopulateDropdowns(vm);
                return View(vm);
            }

            await _propertyService.Add(vm);
            TempData["SuccessMessage"] = vm.IsFeatured ? "Propiedad creada y DESTACADA exitosamente ⭐." : "Propiedad creada exitosamente.";
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

            var canFeature = await _subscriptionService.CanAgentFeaturePropertyAsync(userId, id);
            var currentSub = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(userId);
            ViewBag.CanFeature = canFeature.Allowed || vm.IsFeatured;
            ViewBag.MaxFeatured = currentSub?.MaxFeaturedProperties ?? 0;
            ViewBag.PlanName = currentSub?.PlanName ?? "Gratuito";
            ViewBag.FeatureMessage = canFeature.Message;

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

            if (vm.IsFeatured && !existing.IsFeatured)
            {
                var featureCheck = await _subscriptionService.CanAgentFeaturePropertyAsync(userId, vm.Id);
                if (!featureCheck.Allowed)
                {
                    ModelState.AddModelError("IsFeatured", featureCheck.Message);
                    vm.IsFeatured = false;
                }
            }

            vm.AgentId = userId;

            if (!ModelState.IsValid)
            {
                var canFeature = await _subscriptionService.CanAgentFeaturePropertyAsync(userId, vm.Id);
                var currentSub = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(userId);
                ViewBag.CanFeature = canFeature.Allowed || existing.IsFeatured;
                ViewBag.MaxFeatured = currentSub?.MaxFeaturedProperties ?? 0;
                ViewBag.PlanName = currentSub?.PlanName ?? "Gratuito";
                ViewBag.FeatureMessage = canFeature.Message;

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

            var provinces = await _provinceRepository.GetAllAsync();
            vm.Provinces = provinces.Select(p => new ProvinceDropdownViewModel { Id = p.Id, Name = p.Name, IsoCode = p.IsoCode }).ToList();

            if (vm.ProvinceId.HasValue)
            {
                var municipalities = await _municipalityRepository.GetByProvinceIdAsync(vm.ProvinceId.Value);
                vm.Municipalities = municipalities.Select(m => new MunicipalityDropdownViewModel { Id = m.Id, Name = m.Name, ProvinceId = m.ProvinceId }).ToList();
            }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CounterOffer(CounterOfferViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Datos de contra-oferta inválidos. Verifique el monto ingresado.";
                return RedirectToAction(nameof(Offers));
            }

            try
            {
                await _offerService.CounterOffer(vm.OfferId, vm.CounterOfferAmount, vm.CounterOfferMessage, userId);
                TempData["SuccessMessage"] = $"Contra-oferta por RD$ {vm.CounterOfferAmount:N0} enviada exitosamente al cliente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Offers));
        }

        [HttpGet]
        public async Task<IActionResult> Verification()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = await _verificationService.GetByAgentIdAsync(userId) ?? new AgentVerificationViewModel
            {
                AgentId = userId
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verification(AgentVerificationViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            vm.AgentId = userId;
            ModelState.Remove("AgentId");
            ModelState.Remove("AgentName");
            ModelState.Remove("AgentEmail");
            ModelState.Remove("AgentPhone");
            ModelState.Remove("Status");
            ModelState.Remove("FrontImageUrl");
            ModelState.Remove("BackImageUrl");

            if (string.IsNullOrWhiteSpace(vm.Cedula))
            {
                ModelState.AddModelError("Cedula", "El número de cédula es obligatorio.");
            }
            else
            {
                vm.Cedula = vm.Cedula.Trim();
            }

            var existing = await _verificationService.GetByAgentIdAsync(userId);
            if (existing == null || string.IsNullOrEmpty(existing.FrontImageUrl))
            {
                if (vm.FrontImageFile == null || vm.FrontImageFile.Length == 0)
                {
                    ModelState.AddModelError("FrontImageFile", "Debe adjuntar la foto frontal de su cédula.");
                }
            }

            if (existing == null || string.IsNullOrEmpty(existing.BackImageUrl))
            {
                if (vm.BackImageFile == null || vm.BackImageFile.Length == 0)
                {
                    ModelState.AddModelError("BackImageFile", "Debe adjuntar la foto posterior de su cédula.");
                }
            }

            if (!ModelState.IsValid)
            {
                var current = await _verificationService.GetByAgentIdAsync(userId);
                if (current != null)
                {
                    vm.FrontImageUrl = current.FrontImageUrl;
                    vm.BackImageUrl = current.BackImageUrl;
                    vm.Status = current.Status;
                    vm.RejectionReason = current.RejectionReason;
                }
                return View(vm);
            }

            try
            {
                await _verificationService.SubmitVerificationAsync(vm);
                TempData["SuccessMessage"] = "Solicitud de verificación enviada exitosamente. El equipo de administración revisará sus documentos.";
                return RedirectToAction(nameof(Verification));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al enviar la solicitud: {ex.Message}");
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Subscriptions()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var vm = await _subscriptionService.GetAgentDashboardViewModelAsync(userId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeSubscription(int planId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var success = await _subscriptionService.SubscribeAgentAsync(userId, planId);
            if (success)
            {
                TempData["SuccessMessage"] = "¡Tu plan de suscripción fue actualizado exitosamente! Ahora cuentas con nuevos beneficios.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo actualizar el plan de suscripción seleccionado.";
            }

            return RedirectToAction(nameof(Subscriptions));
        }
    }
}
