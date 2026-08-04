using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Client")]
    public class OffersController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly IPropertyService _propertyService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUserActivityService _userActivityService;
        private readonly UserManager<IdentityUser> _userManager;

        public OffersController(
            IOfferService offerService,
            IPropertyService propertyService,
            IFileStorageService fileStorageService,
            IUserActivityService userActivityService,
            UserManager<IdentityUser> userManager)
        {
            _offerService = offerService;
            _propertyService = propertyService;
            _fileStorageService = fileStorageService;
            _userActivityService = userActivityService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
        {
            var property = await _propertyService.GetByIdViewModel(propertyId);
            if (property == null || property.Status != PropertyStatus.Available)
            {
                TempData["ErrorMessage"] = "Esta propiedad no está disponible para recibir ofertas.";
                return RedirectToAction("Index", "Home");
            }

            var vm = new SaveOfferViewModel
            {
                PropertyId = property.Id,
                PropertyCode = property.Code,
                PropertyPrice = property.Price
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveOfferViewModel vm)
        {
            var property = await _propertyService.GetByIdViewModel(vm.PropertyId);
            if (property == null || property.Status != PropertyStatus.Available)
            {
                ModelState.AddModelError(string.Empty, "La propiedad no se encuentra disponible para ofertas.");
                return View(vm);
            }

            vm.PropertyCode = property.Code;
            vm.PropertyPrice = property.Price;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Procesar la carga de la carta de pre-aprobación bancaria si se adjuntó
            if (vm.PreApprovalLetter != null && vm.PreApprovalLetter.Length > 0)
            {
                using var stream = vm.PreApprovalLetter.OpenReadStream();
                var letterUrl = await _fileStorageService.UploadFileAsync(stream, vm.PreApprovalLetter.FileName, "preapprovals");
                vm.PreApprovalLetterUrl = letterUrl;
            }

            try
            {
                await _offerService.Add(vm, userId);
                await _userActivityService.LogActivityAsync(userId, "Oferta Enviada", $"Realizó una oferta por RD$ {vm.MontoOfertado:N0} en la propiedad (Cód: {property.Code})", "bi-cash-stack text-success", $"/Home/Details/{property.Id}");
                TempData["SuccessMessage"] = "¡Tu propuesta económica ha sido enviada exitosamente al agente!";
                return RedirectToAction(nameof(MyOffers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyOffers()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var offers = await _offerService.GetByClienteId(userId);
            return View(offers);
        }
    }
}
