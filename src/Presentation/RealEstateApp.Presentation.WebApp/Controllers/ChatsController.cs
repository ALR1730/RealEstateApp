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

        public ChatsController(
            IChatService chatService,
            IPropertyService propertyService,
            UserManager<IdentityUser> userManager)
        {
            _chatService = chatService;
            _propertyService = propertyService;
            _userManager = userManager;
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

        public async Task<IActionResult> Thread(int propertyId, string? agentId = null, string? clienteId = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var property = await _propertyService.GetByIdViewModel(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            var currentUserId = userId;
            string targetAgentId = property.AgentId;
            string targetClienteId = currentUserId;

            if (currentUserId == property.AgentId)
            {
                // Quien consulta es el agente del inmueble
                targetClienteId = !string.IsNullOrEmpty(clienteId) ? clienteId : (agentId != property.AgentId && !string.IsNullOrEmpty(agentId) ? agentId : string.Empty);
            }

            var messages = await _chatService.GetChatThread(targetClienteId, targetAgentId, propertyId, currentUserId);

            // Si el cliente aún no está definido (ej. primer render sin mensajes), tomarlo del primer mensaje existente si hubiere
            if (currentUserId == property.AgentId && string.IsNullOrEmpty(targetClienteId) && messages.Count > 0)
            {
                targetClienteId = messages[0].ClienteId;
            }

            ViewBag.Property = property;
            ViewBag.AgentId = targetAgentId;
            ViewBag.ClienteId = targetClienteId;

            var vm = new SaveChatViewModel
            {
                PropertyId = propertyId,
                RecipientId = currentUserId == property.AgentId ? targetClienteId : targetAgentId
            };

            ViewBag.Messages = messages;
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

            var property = await _propertyService.GetByIdViewModel(vm.PropertyId);

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

            var property = await _propertyService.GetByIdViewModel(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            var currentUserId = userId;
            string targetAgentId = property.AgentId;
            string targetClienteId = currentUserId;

            if (currentUserId == property.AgentId)
            {
                targetClienteId = !string.IsNullOrEmpty(clienteId) ? clienteId : (agentId != property.AgentId && !string.IsNullOrEmpty(agentId) ? agentId : string.Empty);
            }

            var messages = await _chatService.GetChatThread(targetClienteId, targetAgentId, propertyId, currentUserId);

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
    }
}
