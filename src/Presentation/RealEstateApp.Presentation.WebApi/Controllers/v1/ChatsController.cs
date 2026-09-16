using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize]
    public class ChatsController : BaseApiController
    {
        private readonly IChatService _chatService;

        public ChatsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Obtiene todas las conversaciones / hilos de chat del usuario autenticado.
        /// </summary>
        [HttpGet("conversations")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatViewModel>))]
        public async Task<IActionResult> GetConversationsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var chats = await _chatService.GetUserChats(userId);
            return Ok(chats);
        }

        /// <summary>
        /// Obtiene el hilo de mensajes para una propiedad específica entre cliente y agente.
        /// </summary>
        [HttpGet("thread")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChatViewModel>))]
        public async Task<IActionResult> GetThreadAsync([FromQuery] string clientId, [FromQuery] string agentId, [FromQuery] int? propertyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var messages = await _chatService.GetChatThread(clientId, agentId, propertyId, userId);
            return Ok(messages);
        }

        /// <summary>
        /// Envía un mensaje de chat vía REST.
        /// </summary>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendMessageAsync([FromBody] SaveChatViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _chatService.SendMessage(model, userId);
            return Ok(new { success = true, message = "Mensaje enviado." });
        }
    }
}
