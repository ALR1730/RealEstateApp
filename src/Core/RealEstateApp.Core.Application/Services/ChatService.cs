using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de chat bidireccional entre cliente y agente por propiedad.
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;

        public ChatService(IChatRepository chatRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _mapper = mapper;
        }

        public async Task<List<ChatViewModel>> GetChatThread(
            string clienteId, string agenteId, int propertyId, string currentUserId)
        {
            var messages = await _chatRepository.GetChatThreadAsync(clienteId, agenteId, propertyId);
            var viewModels = _mapper.Map<List<ChatViewModel>>(messages);

            // Marcar cuáles mensajes son del usuario actual
            foreach (var vm in viewModels)
            {
                vm.IsMine = vm.SenderId == currentUserId;
            }

            return viewModels;
        }

        public async Task SendMessage(SaveChatViewModel vm, string senderId)
        {
            var chat = _mapper.Map<Chat>(vm);
            chat.SenderId = senderId;
            chat.SentAt = DateTime.UtcNow;

            // Determinar roles: si el sender es el RecipientId's agent o client
            // La lógica de asignación de ClienteId/AgenteId se resuelve aquí
            // basándose en quién envía y quién recibe
            chat.ClienteId = vm.RecipientId; // Se ajusta en el controlador según contexto
            chat.AgenteId = senderId;        // Se ajusta en el controlador según contexto

            await _chatRepository.AddAsync(chat);
        }

        public async Task<List<ChatViewModel>> GetUserChats(string userId)
        {
            var messages = await _chatRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<ChatViewModel>>(messages);
        }
    }
}
