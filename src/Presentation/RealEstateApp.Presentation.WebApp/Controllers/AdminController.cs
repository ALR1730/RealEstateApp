using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
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

            ViewBag.TotalAvailableProperties = properties.Count(p => p.Status == PropertyStatus.Available);
            ViewBag.TotalReservedProperties = properties.Count(p => p.Status == PropertyStatus.Reserved);
            ViewBag.TotalSoldProperties = properties.Count(p => p.Status == PropertyStatus.Sold);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleDeveloperStatus(string developerId, bool isActive)
        {
            try
            {
                await _accountService.ChangeUserStatusAsync(developerId, isActive);
                var text = isActive ? "activado" : "inactivado";
                TempData["SuccessMessage"] = $"El desarrollador fue {text} exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Developers));
        }

        [HttpGet]
        public async Task<IActionResult> EditDeveloper(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Developer.ToString()))
            {
                return NotFound();
            }

            var isActive = !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;
            var claims = await _userManager.GetClaimsAsync(user);
            var firstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value ?? string.Empty;
            var lastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value ?? string.Empty;

            var vm = new EditDeveloperViewModel
            {
                Id = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Phone = user.PhoneNumber,
                IsActive = isActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDeveloper(EditDeveloperViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Validar unicidad de Email
            var userByEmail = await _userManager.FindByEmailAsync(vm.Email);
            if (userByEmail != null && userByEmail.Id != user.Id)
            {
                ModelState.AddModelError("Email", $"El correo '{vm.Email}' ya está registrado por otro usuario.");
                return View(vm);
            }

            // Validar unicidad de UserName
            var userByUsername = await _userManager.FindByNameAsync(vm.UserName);
            if (userByUsername != null && userByUsername.Id != user.Id)
            {
                ModelState.AddModelError("UserName", $"El usuario '{vm.UserName}' ya está registrado por otro usuario.");
                return View(vm);
            }

            user.Email = vm.Email;
            user.UserName = vm.UserName;
            user.PhoneNumber = vm.Phone;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(vm);
            }

            // Actualizar claims de Nombre y Apellido
            var currentClaims = await _userManager.GetClaimsAsync(user);
            await UpdateUserClaim(user, currentClaims, "FirstName", vm.FirstName);
            await UpdateUserClaim(user, currentClaims, "LastName", vm.LastName);

            // Cambiar contraseña si se especificó
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, vm.Password);
                if (!resetResult.Succeeded)
                {
                    foreach (var error in resetResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(vm);
                }
            }

            TempData["SuccessMessage"] = "Desarrollador modificado exitosamente.";
            return RedirectToAction(nameof(Developers));
        }

        private async Task UpdateUserClaim(IdentityUser user, IList<System.Security.Claims.Claim> existingClaims, string claimType, string value)
        {
            var existingClaim = existingClaims.FirstOrDefault(c => c.Type == claimType);
            if (existingClaim != null)
            {
                await _userManager.ReplaceClaimAsync(user, existingClaim, new System.Security.Claims.Claim(claimType, value ?? string.Empty));
            }
            else
            {
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(claimType, value ?? string.Empty));
            }
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

        public async Task<IActionResult> Properties()
        {
            var properties = await _propertyService.GetAllViewModel();
            var agents = await _agentService.GetAllViewModelAsync();
            var agentMap = agents.ToDictionary(a => a.Id, a => a.FullName);

            foreach (var prop in properties)
            {
                if (!string.IsNullOrEmpty(prop.AgentId) && agentMap.TryGetValue(prop.AgentId, out var agentName))
                {
                    prop.AgentName = agentName;
                }
                else if (!string.IsNullOrEmpty(prop.AgentId))
                {
                    var agentUser = await _userManager.FindByIdAsync(prop.AgentId);
                    prop.AgentName = agentUser?.UserName ?? agentUser?.Email ?? "Agente Desconocido";
                }
                else
                {
                    prop.AgentName = "Sin Agente Asignado";
                }
            }

            return View(properties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            try
            {
                await _propertyService.Delete(id);
                TempData["SuccessMessage"] = "La propiedad fue eliminada exitosamente por el administrador.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Properties));
        }

        [HttpGet]
        public async Task<IActionResult> ReassignProperty(int id)
        {
            var property = await _propertyService.GetByIdViewModel(id);
            if (property == null)
            {
                return NotFound();
            }

            var agents = await _agentService.GetAllViewModelAsync();
            var currentAgent = agents.FirstOrDefault(a => a.Id == property.AgentId);

            var vm = new ReassignPropertyViewModel
            {
                PropertyId = property.Id,
                PropertyCode = property.Code,
                PropertyTypeName = property.PropertyTypeName,
                Price = property.Price,
                CurrentAgentId = property.AgentId,
                CurrentAgentName = currentAgent != null ? currentAgent.FullName : "Sin Agente Asignado",
                NewAgentId = property.AgentId,
                Agents = agents.Where(a => a.IsActive).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignProperty(ReassignPropertyViewModel vm)
        {
            if (string.IsNullOrEmpty(vm.NewAgentId))
            {
                ModelState.AddModelError(nameof(vm.NewAgentId), "Debe seleccionar un agente válido.");
            }

            if (!ModelState.IsValid)
            {
                var agents = await _agentService.GetAllViewModelAsync();
                vm.Agents = agents.Where(a => a.IsActive).ToList();
                return View(vm);
            }

            try
            {
                await _propertyService.ReassignAgent(vm.PropertyId, vm.NewAgentId);
                TempData["SuccessMessage"] = "La propiedad fue reasignada exitosamente al nuevo agente.";
                return RedirectToAction(nameof(Properties));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                var agents = await _agentService.GetAllViewModelAsync();
                vm.Agents = agents.Where(a => a.IsActive).ToList();
                return View(vm);
            }
        }
    }
}
