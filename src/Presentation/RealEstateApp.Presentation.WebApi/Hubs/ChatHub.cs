using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;

namespace RealEstateApp.Presentation.WebApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private const int MaxMessageLength = 2000;
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinThread(int propertyId, string recipientId)
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(recipientId))
                throw new HubException("Parámetros de hilo inválidos.");

            if (!await _chatService.CanUserAccessThreadAsync(propertyId, userId, recipientId))
                throw new HubException("No autorizado para acceder a este hilo de chat.");

            string groupName = GetGroupName(propertyId, userId, recipientId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveThread(int propertyId, string recipientId)
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(recipientId)) return;

            string groupName = GetGroupName(propertyId, userId, recipientId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessage(int propertyId, string recipientId, string messageContent)
        {
            var senderId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(recipientId) || string.IsNullOrWhiteSpace(messageContent))
                throw new HubException("El mensaje y el destinatario son obligatorios.");

            if (messageContent.Length > MaxMessageLength)
                throw new HubException($"El mensaje no puede superar los {MaxMessageLength} caracteres.");

            if (!await _chatService.CanUserAccessThreadAsync(propertyId, senderId, recipientId))
                throw new HubException("No autorizado para enviar mensajes en este hilo de chat.");

            var vm = new SaveChatViewModel
            {
                PropertyId = propertyId,
                RecipientId = recipientId,
                MessageContent = messageContent
            };

            await _chatService.SendMessage(vm, senderId);

            string groupName = GetGroupName(propertyId, senderId, recipientId);
            var utcNow = DateTime.UtcNow;

            var messagePayload = new
            {
                propertyId = propertyId,
                senderId = senderId,
                recipientId = recipientId,
                messageContent = WebUtility.HtmlEncode(messageContent),
                sentAtUtc = utcNow.ToString("O"),
                sentAtFormatted = utcNow.ToString("hh:mm tt | dd/MM")
            };

            await Clients.Group(groupName).SendAsync("ReceiveMessage", messagePayload);
            await Clients.User(recipientId).SendAsync("NewChatMessageNotification", messagePayload);
        }

        public async Task SendTyping(int propertyId, string recipientId, bool isTyping)
        {
            var senderId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(recipientId)) return;

            if (!await _chatService.CanUserAccessThreadAsync(propertyId, senderId, recipientId)) return;

            string groupName = GetGroupName(propertyId, senderId, recipientId);
            await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new { senderId, isTyping });
        }

        private static string GetGroupName(int propertyId, string userId1, string userId2)
        {
            string userA = string.Compare(userId1, userId2, StringComparison.Ordinal) < 0 ? userId1 : userId2;
            string userB = string.Compare(userId1, userId2, StringComparison.Ordinal) < 0 ? userId2 : userId1;
            return $"chat_{propertyId}_{userA}_{userB}";
        }
    }
}
