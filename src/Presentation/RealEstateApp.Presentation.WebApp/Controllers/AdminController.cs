using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAgentService _agentService;
        private readonly IPropertyService _propertyService;
        private readonly IAccountService _accountService;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            IAgentService agentService,
            IPropertyService propertyService,
            IAccountService accountService,
            UserManager<IdentityUser> userManager)
        {
            _agentService = agentService;
            _propertyService = propertyService;
            _accountService = accountService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var properties = await _propertyService.GetAllViewModel();
            var agents = await _agentService.GetAllViewModelAsync();
            var clients = await _userManager.GetUsersInRoleAsync(Roles.Client.ToString());
            var developers = await _userManager.GetUsersInRoleAsync(Roles.Developer.ToString());

            ViewBag.TotalAvailableProperties = properties.Count(p => p.Status == "Disponible");
            ViewBag.TotalReservedProperties = properties.Count(p => p.Status == "Reservada");
            ViewBag.TotalSoldProperties = properties.Count(p => p.Status == "Vendida");
            ViewBag.TotalActiveAgents = agents.Count(a => a.IsActive);
            ViewBag.TotalInactiveAgents = agents.Count(a => !a.IsActive);
            ViewBag.TotalClients = clients.Count;
            ViewBag.TotalDevelopers = developers.Count;

            return View();
        }

        public async Task<IActionResult> Agents()
        {
            var agents = await _agentService.GetAllViewModelAsync();
            return View(agents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAgentStatus(string agentId, bool isActive)
        {
            try
            {
                await _agentService.ChangeStatusAsync(agentId, isActive);
                var text = isActive ? "activado" : "inactivado";
                TempData["SuccessMessage"] = $"El agente fue {text} exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Agents));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAgent(string id)
        {
            var agent = await _agentService.GetByIdViewModelAsync(id);
            if (agent == null)
            {
                return NotFound();
            }

            return View(agent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteAgent(string id)
        {
            try
            {
                await _agentService.DeleteAgentCascadeAsync(id);
                TempData["SuccessMessage"] = "El agente y todas sus propiedades, ofertas, favoritos e imágenes adjuntas fueron eliminados físicamente en cascada.";
                return RedirectToAction(nameof(Agents));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Agents));
            }
        }

        public async Task<IActionResult> Developers()
        {
            var developers = await _userManager.GetUsersInRoleAsync(Roles.Developer.ToString());
            return View(developers);
        }

        [HttpGet]
        public IActionResult CreateDeveloper()
        {
            return View(new RegisterViewModel { UserType = "Developer" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeveloper(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var request = new RegisterRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                UserName = vm.UserName,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword,
                Phone = vm.Phone
            };

            var response = await _accountService.RegisterUserAsync(request, Roles.Developer.ToString());
            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty, response.Error ?? "Error al registrar desarrollador.");
                return View(vm);
            }

            TempData["SuccessMessage"] = "Desarrollador registrado exitosamente.";
            return RedirectToAction(nameof(Developers));
        }

        public async Task<IActionResult> Admins()
        {
            var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin.ToString());
            return View(admins);
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            return View(new RegisterViewModel { UserType = "Admin" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var request = new RegisterRequest
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                UserName = vm.UserName,
                Password = vm.Password,
                ConfirmPassword = vm.ConfirmPassword,
                Phone = vm.Phone
            };

            var response = await _accountService.RegisterUserAsync(request, Roles.Admin.ToString());
            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty, response.Error ?? "Error al registrar administrador.");
                return View(vm);
            }

            TempData["SuccessMessage"] = "Administrador registrado exitosamente.";
            return RedirectToAction(nameof(Admins));
        }
    }
}
