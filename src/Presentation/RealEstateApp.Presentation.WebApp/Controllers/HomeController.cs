using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISaleTypeService _saleTypeService;
        private readonly IImprovementService _improvementService;
        private readonly IFavoriteService _favoriteService;
        private readonly IFinancingService _financingService;
        private readonly IOfferService _offerService;
        private readonly IUserActivityService _userActivityService;
        private readonly IProvinceRepository _provinceRepository;
        private readonly IMunicipalityRepository _municipalityRepository;
        private readonly IAgentVerificationService _verificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            IPropertyService propertyService,
            IPropertyTypeService propertyTypeService,
            ISaleTypeService saleTypeService,
            IImprovementService improvementService,
            IFavoriteService favoriteService,
            IFinancingService financingService,
            IOfferService offerService,
            IUserActivityService userActivityService,
            IProvinceRepository provinceRepository,
            IMunicipalityRepository municipalityRepository,
            IAgentVerificationService verificationService,
            UserManager<IdentityUser> userManager)
        {
            _propertyService = propertyService;
            _propertyTypeService = propertyTypeService;
            _saleTypeService = saleTypeService;
            _improvementService = improvementService;
            _favoriteService = favoriteService;
            _financingService = financingService;
            _offerService = offerService;
            _userActivityService = userActivityService;
            _provinceRepository = provinceRepository;
            _municipalityRepository = municipalityRepository;
            _verificationService = verificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(PropertyFilterViewModel filters)
        {
            // Cargar propiedades filtradas
            var properties = await _propertyService.GetAllWithFilters(filters);

            // Filtrar por radio geográfico Haversine si se proveen coordenadas y distancia máxima
            if (filters.UserLat.HasValue && filters.UserLng.HasValue && filters.MaxDistanceKm.HasValue && filters.MaxDistanceKm.Value > 0)
            {
                double userLat = filters.UserLat.Value;
                double userLng = filters.UserLng.Value;
                double maxKm = filters.MaxDistanceKm.Value;

                properties = properties.Where(p =>
                {
                    if (p.Latitude == 0 && p.Longitude == 0) return false;
                    double dist = CalculateHaversineDistance(userLat, userLng, p.Latitude, p.Longitude);
                    return dist <= maxKm;
                }).ToList();
            }

            // Las propiedades vendidas solo son visibles para Agentes, Administradores y Desarrolladores
            var isAgentOrAdmin = User.Identity != null && User.Identity.IsAuthenticated && (User.IsInRole("Agent") || User.IsInRole("Admin") || User.IsInRole("Developer"));
            if (!isAgentOrAdmin)
            {
                properties = properties.Where(p => p.Status != PropertyStatus.Sold).ToList();
            }

            // Poblar dropdowns para el formulario de filtro
            var propTypes = await _propertyTypeService.GetAllViewModel();
            filters.PropertyTypes = propTypes.Select(pt => new RealEstateApp.Core.Application.ViewModels.Property.PropertyTypeViewModel { Id = pt.Id, Name = pt.Name }).ToList();

            var saleTypes = await _saleTypeService.GetAllViewModel();
            filters.SaleTypes = saleTypes.Select(st => new RealEstateApp.Core.Application.ViewModels.Property.SaleTypeViewModel { Id = st.Id, Name = st.Name }).ToList();

            var improvements = await _improvementService.GetAllViewModel();
            filters.AvailableImprovements = improvements.Select(i => new RealEstateApp.Core.Application.ViewModels.Property.ImprovementViewModel { Id = i.Id, Name = i.Name }).ToList();

            var provinces = await _provinceRepository.GetAllAsync();
            filters.Provinces = provinces.Select(p => new ProvinceDropdownViewModel { Id = p.Id, Name = p.Name, IsoCode = p.IsoCode }).ToList();

            if (filters.ProvinceId.HasValue)
            {
                var municipalities = await _municipalityRepository.GetByProvinceIdAsync(filters.ProvinceId.Value);
                filters.Municipalities = municipalities.Select(m => new MunicipalityDropdownViewModel { Id = m.Id, Name = m.Name, ProvinceId = m.ProvinceId }).ToList();
            }

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

            // Resolver estado de verificación de identidad de los agentes
            var verifications = await _verificationService.GetAllAsync();
            var verifiedAgentIds = verifications
                .Where(v => v.Status == "Approved" || v.Status == RealEstateApp.Core.Domain.Constants.VerificationStatus.Approved)
                .Select(v => v.AgentId)
                .ToHashSet();

            foreach (var prop in properties)
            {
                prop.IsAgentVerified = verifiedAgentIds.Contains(prop.AgentId);
            }

            ViewBag.Filters = filters;
            return View(properties);
        }

        [HttpGet]
        public async Task<IActionResult> GetMunicipalities(int provinceId)
        {
            var municipalities = await _municipalityRepository.GetByProvinceIdAsync(provinceId);
            return Json(municipalities.Select(m => new { id = m.Id, name = m.Name }));
        }

        public async Task<IActionResult> Map()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMapData()
        {
            var properties = await _propertyService.GetAllWithFilters(new PropertyFilterViewModel());
            properties = properties.Where(p => p.Status != PropertyStatus.Sold).ToList();

            var result = properties.Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                p.Price,
                PriceFormatted = p.Price.ToString("N0"),
                p.Rooms,
                p.Bathrooms,
                Size = p.SizeInMeters.ToString("N0"),
                PropertyType = p.PropertyTypeName,
                Province = p.ProvinceName,
                Municipality = p.MunicipalityName,
                p.Sector,
                p.Latitude,
                p.Longitude,
                ImageUrl = p.Images.FirstOrDefault() ?? "/images/default-property.jpg"
            });

            return Json(result);
        }

        public async Task<IActionResult> Compare(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return View(new List<PropertyViewModel>());
            }

            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(idStr => int.TryParse(idStr, out int val) ? val : 0)
                            .Where(val => val > 0)
                            .Distinct()
                            .Take(4)
                            .ToList();

            var result = new List<PropertyViewModel>();
            foreach (var id in idList)
            {
                var prop = await _propertyService.GetByIdViewModel(id);
                if (prop != null)
                {
                    result.Add(prop);
                }
            }

            return View(result);
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

            if (property.Status == PropertyStatus.Sold && !isAgentOrAdmin && !isBuyerWithAcceptedOffer)
            {
                return NotFound();
            }

            // Obtener el nombre del agente/propietario y estado de verificación
            var agentUser = await _userManager.FindByIdAsync(property.AgentId);
            bool isOwnerPublisher = false;
            if (agentUser != null)
            {
                property.AgentName = agentUser.UserName ?? agentUser.Email ?? "Contacto";
                isOwnerPublisher = await _userManager.IsInRoleAsync(agentUser, Roles.Owner.ToString());
            }
            ViewBag.IsOwnerPublisher = isOwnerPublisher;
            ViewBag.IsAgentVerified = await _verificationService.IsAgentVerifiedAsync(property.AgentId);

            // Resolver si es favorita para el usuario actual y registrar actividad
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUserId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    if (User.IsInRole("Client"))
                    {
                        property.IsFavorite = await _favoriteService.IsFavorite(currentUserId, property.Id);
                    }
                    await _userActivityService.LogActivityAsync(currentUserId, "Detalles de Inmueble", $"Visualizó la propiedad '{property.Name}' (Cód: {property.Code})", "bi-eye-fill", $"/Home/Details/{id}");
                }
            }

            return View(property);
        }

        /// <summary>
        /// Calcula la amortización e información hipotecaria basada en los parámetros ingresados.
        /// </summary>
        [HttpPost]
        public IActionResult CalculateMortgage(decimal price, decimal downPayment, decimal rate, int years)
        {
            var simulation = _financingService.CalculateMortgage(price, downPayment, rate, years);
            return Json(simulation);
        }

        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radio de la Tierra en kilómetros
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double val)
        {
            return (Math.PI / 180) * val;
        }
    }
}
