using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de chat bidireccional entre cliente y agente por propiedad.
    /// </summary>
    public class ChatService : IChatService
    {
        private const int MaxMessageLength = 2000;

        private readonly IChatRepository _chatRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public ChatService(
            IChatRepository chatRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper)
        {
            _chatRepository = chatRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<List<ChatViewModel>> GetChatThread(
            string clienteId, string agenteId, int? propertyId, string currentUserId)
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
            if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(vm.RecipientId) || senderId == vm.RecipientId)
            {
                throw new ValidationException("Los participantes del chat son inválidos.");
            }

            if (string.IsNullOrWhiteSpace(vm.MessageContent))
            {
                throw new ValidationException("El mensaje no puede estar vacío.");
            }

            if (vm.MessageContent.Length > MaxMessageLength)
            {
                throw new ValidationException($"El mensaje no puede superar los {MaxMessageLength} caracteres.");
            }

            var chat = _mapper.Map<Chat>(vm);
            chat.SenderId = senderId;
            chat.SentAt = DateTime.UtcNow;
            chat.IsWhatsApp = true;
            chat.MessageContent = System.Net.WebUtility.HtmlEncode(vm.MessageContent);

            // Manejo de Soporte: PropertyId <= 0 se mapea a null en la BD
            if (vm.PropertyId.HasValue && vm.PropertyId.Value <= 0)
            {
                chat.PropertyId = null;
                chat.ClienteId = vm.RecipientId;
                chat.AgenteId = senderId;
            }
            else if (vm.PropertyId.HasValue)
            {
                var property = await _propertyRepository.GetByIdAsync(vm.PropertyId.Value);
                if (property == null)
                {
                    throw new NotFoundException("La propiedad especificada no existe.");
                }

                if (senderId != property.AgentId && vm.RecipientId != property.AgentId)
                {
                    throw new ValidationException("No tiene permisos para enviar mensajes en el hilo de esta propiedad.");
                }

                if (senderId == property.AgentId)
                {
                    chat.AgenteId = senderId;
                    chat.ClienteId = vm.RecipientId;
                }
                else
                {
                    chat.AgenteId = property.AgentId;
                    chat.ClienteId = senderId;
                }
            }
            else
            {
                chat.PropertyId = null;
                chat.ClienteId = vm.RecipientId;
                chat.AgenteId = senderId;
            }

            await _chatRepository.AddAsync(chat);
        }

        public async Task<bool> CanUserAccessThreadAsync(int propertyId, string userId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(recipientId) || userId == recipientId)
            {
                return false;
            }

            if (propertyId <= 0)
            {
                return true;
            }

            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
            {
                return false;
            }

            return userId == property.AgentId || recipientId == property.AgentId;
        }

        public async Task<List<ChatViewModel>> GetUserChats(string userId)
        {
            var messages = await _chatRepository.GetByUserIdAsync(userId);
            return _mapper.Map<List<ChatViewModel>>(messages);
        }
    }
}
