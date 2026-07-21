using System;
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

        public async Task<IActionResult> Thread(int propertyId, string agentId)
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

            // Si quien consulta es el propio agente de la propiedad, el recipient es el cliente del chat
            var currentUserId = userId;
            var targetAgentId = string.IsNullOrEmpty(agentId) ? property.AgentId : agentId;
            var targetClienteId = currentUserId == targetAgentId ? agentId : currentUserId;

            var messages = await _chatService.GetChatThread(targetClienteId, targetAgentId, propertyId, currentUserId);

            ViewBag.Property = property;
            ViewBag.AgentId = targetAgentId;

            var vm = new SaveChatViewModel
            {
                PropertyId = propertyId,
                RecipientId = currentUserId == targetAgentId ? targetClienteId : targetAgentId
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

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Thread), new { propertyId = vm.PropertyId, agentId = vm.RecipientId });
            }

            await _chatService.SendMessage(vm, userId);
            return RedirectToAction(nameof(Thread), new { propertyId = vm.PropertyId, agentId = vm.RecipientId });
        }
    }
}
