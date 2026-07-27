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
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;
        private readonly IWhatsAppService _whatsAppService;

        public ChatService(
            IChatRepository chatRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper,
            IWhatsAppService whatsAppService)
        {
            _chatRepository = chatRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
            _whatsAppService = whatsAppService;
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
            chat.IsWhatsApp = true;

            var property = await _propertyRepository.GetByIdAsync(vm.PropertyId);

            if (property != null)
            {
                if (senderId == property.AgentId)
                {
                    // El emisor es el agente de la propiedad
                    chat.AgenteId = senderId;
                    chat.ClienteId = vm.RecipientId;
                }
                else
                {
                    // El emisor es el cliente
                    chat.AgenteId = property.AgentId;
                    chat.ClienteId = senderId;
                }
            }
            else
            {
                chat.ClienteId = vm.RecipientId;
                chat.AgenteId = senderId;
            }

            await _chatRepository.AddAsync(chat);

            // Despachar vía WhatsApp API / Servicio
            string recipientPhone = chat.ClienteId;
            await _whatsAppService.SendWhatsAppMessageAsync(recipientPhone, vm.MessageContent);
        }

        public async Task<List<ChatViewModel>> GetUserChats(string userId)
        {
            var messages = await _chatRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<ChatViewModel>>(messages);
        }
    }
}
