using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IFavoriteService _favoriteService;
        private readonly IFinancingService _financingService;
        private readonly IOfferService _offerService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IFavoriteService favoriteService,
            IFinancingService financingService,
            IOfferService offerService,
            UserManager<IdentityUser> userManager)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _favoriteService = favoriteService;
            _financingService = financingService;
            _offerService = offerService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(PropertyFilterViewModel filters)
        {
            // Cargar propiedades filtradas
            var properties = await _propertyService.GetAllWithFilters(filters);

            // Las propiedades vendidas solo son visibles para Agentes, Administradores y Desarrolladores
            var isAgentOrAdmin = User.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Agent") || User.IsInRole("Admin") || User.IsInRole("Developer"));
            if (!isAgentOrAdmin)
            {
                properties = properties.Where(p => p.Status != "Vendida").ToList();
            }

            // Poblar dropdowns para el formulario de filtro
            var propTypes = await _propertyTypeService.GetAllViewModel();
            filters.PropertyTypes = propTypes.Select(pt => new RealEstateApp.Core.Application.ViewModels.Property.PropertyTypeViewModel { Id = pt.Id, Name = pt.Name }).ToList();

            var saleTypes = await _saleTypeService.GetAllViewModel();
            filters.SaleTypes = saleTypes.Select(st => new RealEstateApp.Core.Application.ViewModels.Property.SaleTypeViewModel { Id = st.Id, Name = st.Name }).ToList();

            // Si el usuario actual está autenticado como Cliente, resolver cuáles propiedades tiene como favoritas
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Client"))
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var userFavorites = await _favoriteService.GetByClienteId(userId);
                    var favPropIds = userFavorites.Select(f => f.PropertyId).ToHashSet();

                    foreach (var prop in properties)
                    {
                        prop.IsFavorite = favPropIds.Contains(prop.Id);
                    }
                }
            }

            ViewBag.Filters = filters;
            return View(properties);
        }

        public async Task<IActionResult> Details(int id)
        {
            var property = await _propertyService.GetByIdViewModel(id);
            if (property == null)
            {
                return NotFound();
            }

            var isAgentOrAdmin = User.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Agent") || User.IsInRole("Admin") || User.IsInRole("Developer"));

            bool isBuyerWithAcceptedOffer = false;
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Client"))
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    var userOffers = await _offerService.GetByClienteId(userId);
                    isBuyerWithAcceptedOffer = userOffers.Any(o => o.PropertyId == id && 
                        (string.Equals(o.Status, "Accepted", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(o.Status, "Aceptada", StringComparison.OrdinalIgnoreCase)));
                }
            }

            ViewBag.IsBuyerWithAcceptedOffer = isBuyerWithAcceptedOffer;

            if (property.Status == "Vendida" && !isAgentOrAdmin && !isBuyerWithAcceptedOffer)
            {
                return NotFound();
            }

            // Obtener el nombre del agente
            var agentUser = await _userManager.FindByIdAsync(property.AgentId);
            if (agentUser != null)
            {
                property.AgentName = agentUser.UserName ?? agentUser.Email ?? "Agente";
            }

            // Resolver si es favorita para el usuario actual
            if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Client"))
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    property.IsFavorite = await _favoriteService.IsFavorite(userId, property.Id);
                }
            }

            return View(property);
        }

        [HttpPost]
        public IActionResult CalculateMortgage(decimal price, decimal downPayment, decimal rate, int years)
        {
            var schedule = _financingService.GenerateAmortizationSchedule(price, downPayment, rate, years);
            return Json(schedule);
        }
    }
}
