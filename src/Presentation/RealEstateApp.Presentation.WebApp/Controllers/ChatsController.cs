using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize]
    public class ChatsController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IPropertyService _propertyService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWhatsAppService _whatsAppService;

        public ChatsController(
            IChatService chatService,
            IPropertyService propertyService,
            UserManager<IdentityUser> userManager,
            IWhatsAppService whatsAppService)
        {
            _chatService = chatService;
            _propertyService = propertyService;
            _userManager = userManager;
            _whatsAppService = whatsAppService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var chats = await _chatService.GetUserChats(userId);
            return View(chats);
        }

        public async Task<IActionResult> DeveloperSupport()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var developers = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Developer.ToString());
            var developer = developers.FirstOrDefault();

            if (developer == null)
            {
                TempData["ErrorMessage"] = "No hay desarrolladores disponibles en el sistema en este momento.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Thread), new { propertyId = 0, agentId = developer.Id, clienteId = userId });
        }

        public async Task<IActionResult> AdminSupport()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var admins = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Admin.ToString());
            var admin = admins.FirstOrDefault();

            if (admin == null)
            {
                TempData["ErrorMessage"] = "No hay administradores disponibles en este momento.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Thread), new { propertyId = -1, agentId = admin.Id, clienteId = userId });
        }

        public async Task<IActionResult> Thread(int propertyId, string? agentId = null, string? clienteId = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Hilos de Soporte a Administración (PropertyId == -1)
            if (propertyId == -1)
            {
                var admins = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Admin.ToString());
                var adminUser = admins.FirstOrDefault();
                var currentUserId = userId;

                string targetAgentId = !string.IsNullOrEmpty(agentId) ? agentId : (adminUser != null ? adminUser.Id : string.Empty);
                string targetClienteId = currentUserId;

                bool isCurrentUserAdmin = admins.Any(a => a.Id == currentUserId);
                if (isCurrentUserAdmin)
                {
                    targetAgentId = currentUserId;
                    targetClienteId = !string.IsNullOrEmpty(clienteId) ? clienteId : string.Empty;
                }

                var messages = await _chatService.GetChatThread(targetClienteId, targetAgentId, -1, currentUserId);
                if (isCurrentUserAdmin && string.IsNullOrEmpty(targetClienteId) && messages.Count > 0)
                {
                    targetClienteId = messages[0].ClienteId;
                }

                var dummyProperty = new RealEstateApp.Core.Application.ViewModels.Property.PropertyViewModel
                {
                    Id = -1,
                    Code = "SOPORTE-ADMIN",
                    Description = "Canal directo de comunicación con la Administración del Sistema."
                };

                ViewBag.Property = dummyProperty;
                ViewBag.AgentId = targetAgentId;
                ViewBag.ClienteId = targetClienteId;

                var vmSupport = new SaveChatViewModel
                {
                    PropertyId = -1,
                    RecipientId = isCurrentUserAdmin ? targetClienteId : targetAgentId
                };

                ViewBag.Messages = messages;
                return View(vmSupport);
            }

            // Hilos de Soporte Técnico a Desarrolladores (PropertyId == 0)
            if (propertyId == 0)
            {
                var developers = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Developer.ToString());
                var devUser = developers.FirstOrDefault();
                var currentUserId = userId;

                string targetAgentId = !string.IsNullOrEmpty(agentId) ? agentId : (devUser != null ? devUser.Id : string.Empty);
                string targetClienteId = currentUserId;

                bool isCurrentUserDev = developers.Any(d => d.Id == currentUserId);
                if (isCurrentUserDev)
                {
                    targetAgentId = currentUserId;
                    targetClienteId = !string.IsNullOrEmpty(clienteId) ? clienteId : string.Empty;
                }

                var messages = await _chatService.GetChatThread(targetClienteId, targetAgentId, 0, currentUserId);
                if (isCurrentUserDev && string.IsNullOrEmpty(targetClienteId) && messages.Count > 0)
                {
                    targetClienteId = messages[0].ClienteId;
                }

                var dummyProperty = new RealEstateApp.Core.Application.ViewModels.Property.PropertyViewModel
                {
                    Id = 0,
                    Code = "SOPORTE-DEV",
                    Description = "Canal de Soporte Técnico y Solicitudes con el Equipo de Desarrolladores."
                };

                ViewBag.Property = dummyProperty;
                ViewBag.AgentId = targetAgentId;
                ViewBag.ClienteId = targetClienteId;

                var vmSupport = new SaveChatViewModel
                {
                    PropertyId = 0,
                    RecipientId = isCurrentUserDev ? targetClienteId : targetAgentId
                };

                ViewBag.Messages = messages;
                return View(vmSupport);
            }

            var property = await _propertyService.GetByIdViewModel(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            var currentUserIdProp = userId;
            string targetAgentIdProp = property.AgentId;
            string targetClienteIdProp = currentUserIdProp;

            if (currentUserIdProp == property.AgentId)
            {
                // Quien consulta es el agente del inmueble
                targetClienteIdProp = !string.IsNullOrEmpty(clienteId) ? clienteId : (agentId != property.AgentId && !string.IsNullOrEmpty(agentId) ? agentId : string.Empty);
            }

            var messagesProp = await _chatService.GetChatThread(targetClienteIdProp, targetAgentIdProp, propertyId, currentUserIdProp);

            // Si el cliente aún no está definido (ej. primer render sin mensajes), tomarlo del primer mensaje existente si hubiere
            if (currentUserIdProp == property.AgentId && string.IsNullOrEmpty(targetClienteIdProp) && messagesProp.Count > 0)
            {
                targetClienteIdProp = messagesProp[0].ClienteId;
            }

            ViewBag.Property = property;
            ViewBag.AgentId = targetAgentIdProp;
            ViewBag.ClienteId = targetClienteIdProp;

            var vm = new SaveChatViewModel
            {
                PropertyId = propertyId,
                RecipientId = currentUserIdProp == property.AgentId ? targetClienteIdProp : targetAgentIdProp
            };

            ViewBag.Messages = messagesProp;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(SaveChatViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (vm.PropertyId == -1)
            {
                if (!ModelState.IsValid)
                {
                    var adminsList = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Admin.ToString());
                    bool isAdminUser = adminsList.Any(a => a.Id == userId);
                    string targetAdminAgent = isAdminUser ? userId : vm.RecipientId;
                    string targetAdminClient = isAdminUser ? vm.RecipientId : userId;
                    var messagesList = await _chatService.GetChatThread(targetAdminClient, targetAdminAgent, -1, userId);

                    ViewBag.Property = new RealEstateApp.Core.Application.ViewModels.Property.PropertyViewModel { Id = -1, Code = "SOPORTE-ADMIN", Description = "Canal de Soporte a Administración" };
                    ViewBag.AgentId = targetAdminAgent;
                    ViewBag.ClienteId = targetAdminClient;
                    ViewBag.Messages = messagesList;
                    return View("Thread", vm);
                }

                await _chatService.SendMessage(vm, userId);
                var admins = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Admin.ToString());
                bool isAdmin = admins.Any(a => a.Id == userId);
                string targetAgent = isAdmin ? userId : vm.RecipientId;
                string targetClient = isAdmin ? vm.RecipientId : userId;
                return RedirectToAction(nameof(Thread), new { propertyId = -1, agentId = targetAgent, clienteId = targetClient });
            }

            if (vm.PropertyId == 0)
            {
                if (!ModelState.IsValid)
                {
                    var developersList = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Developer.ToString());
                    bool isDevUser = developersList.Any(d => d.Id == userId);
                    string targetDevAgent = isDevUser ? userId : vm.RecipientId;
                    string targetDevClient = isDevUser ? vm.RecipientId : userId;
                    var messagesList = await _chatService.GetChatThread(targetDevClient, targetDevAgent, 0, userId);

                    ViewBag.Property = new RealEstateApp.Core.Application.ViewModels.Property.PropertyViewModel { Id = 0, Code = "SOPORTE-DEV", Description = "Canal de Soporte Técnico y Solicitudes" };
                    ViewBag.AgentId = targetDevAgent;
                    ViewBag.ClienteId = targetDevClient;
                    ViewBag.Messages = messagesList;
                    return View("Thread", vm);
                }

                await _chatService.SendMessage(vm, userId);
                var developers = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Developer.ToString());
                bool isDev = developers.Any(d => d.Id == userId);
                string targetAgent = isDev ? userId : vm.RecipientId;
                string targetClient = isDev ? vm.RecipientId : userId;
                return RedirectToAction(nameof(Thread), new { propertyId = 0, agentId = targetAgent, clienteId = targetClient });
            }

            var property = await _propertyService.GetByIdViewModel(vm.PropertyId ?? 0);

            if (!ModelState.IsValid)
            {
                string targetAgent = property?.AgentId ?? string.Empty;
                string targetClient = userId == targetAgent ? vm.RecipientId : userId;
                var threadMessages = await _chatService.GetChatThread(targetClient, targetAgent, vm.PropertyId, userId);

                ViewBag.Property = property;
                ViewBag.AgentId = targetAgent;
                ViewBag.ClienteId = targetClient;
                ViewBag.Messages = threadMessages;

                return View("Thread", vm);
            }

            await _chatService.SendMessage(vm, userId);

            string? targetCliente = userId == property?.AgentId ? vm.RecipientId : userId;
            return RedirectToAction(nameof(Thread), new { propertyId = vm.PropertyId, clienteId = targetCliente, agentId = property?.AgentId });
        }

        [HttpGet]
        public async Task<IActionResult> GetThreadMessagesJson(int propertyId, string? agentId = null, string? clienteId = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (propertyId == -1)
            {
                var admins = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Admin.ToString());
                bool isAdmin = admins.Any(a => a.Id == userId);
                string targetAgentId = isAdmin ? userId : (!string.IsNullOrEmpty(agentId) ? agentId : (admins.FirstOrDefault()?.Id ?? string.Empty));
                string targetClienteId = isAdmin ? (!string.IsNullOrEmpty(clienteId) ? clienteId : string.Empty) : userId;

                var adminMessages = await _chatService.GetChatThread(targetClienteId, targetAgentId, -1, userId);
                var adminResult = adminMessages.Select(m => new
                {
                    m.Id,
                    m.MessageContent,
                    SentAtFormatted = m.SentAt.ToString("hh:mm tt | dd/MM"),
                    m.IsMine,
                    m.IsWhatsApp,
                    m.SenderId
                });
                return Json(adminResult);
            }

            if (propertyId == 0)
            {
                var developers = await _userManager.GetUsersInRoleAsync(RealEstateApp.Core.Domain.Enums.Roles.Developer.ToString());
                bool isDev = developers.Any(d => d.Id == userId);
                string targetAgentId = isDev ? userId : (!string.IsNullOrEmpty(agentId) ? agentId : (developers.FirstOrDefault()?.Id ?? string.Empty));
                string targetClienteId = isDev ? (!string.IsNullOrEmpty(clienteId) ? clienteId : string.Empty) : userId;

                var devMessages = await _chatService.GetChatThread(targetClienteId, targetAgentId, 0, userId);
                var devResult = devMessages.Select(m => new
                {
                    m.Id,
                    m.MessageContent,
                    SentAtFormatted = m.SentAt.ToString("hh:mm tt | dd/MM"),
                    m.IsMine,
                    m.IsWhatsApp,
                    m.SenderId
                });
                return Json(devResult);
            }

            var property = await _propertyService.GetByIdViewModel(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            var currentUserId = userId;
            string targetAgentIdProp = property.AgentId;
            string targetClienteIdProp = currentUserId;

            if (currentUserId == property.AgentId)
            {
                targetClienteIdProp = !string.IsNullOrEmpty(clienteId) ? clienteId : (agentId != property.AgentId && !string.IsNullOrEmpty(agentId) ? agentId : string.Empty);
            }

            var messages = await _chatService.GetChatThread(targetClienteIdProp, targetAgentIdProp, propertyId, currentUserId);

            var result = messages.Select(m => new
            {
                m.Id,
                m.MessageContent,
                SentAtFormatted = m.SentAt.ToString("hh:mm tt | dd/MM"),
                m.IsMine,
                m.IsWhatsApp,
                m.SenderId
            });

            return Json(result);
        }

        [HttpGet]
        public IActionResult GetWhatsAppUrl(string phone, string message)
        {
            var url = _whatsAppService.GenerateWhatsAppUrl(phone, message);
            return Json(new { url });
        }
    }
}
