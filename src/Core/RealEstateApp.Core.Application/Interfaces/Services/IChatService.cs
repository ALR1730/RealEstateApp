using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Chat;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para el módulo de chat bidireccional.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Obtiene el hilo de mensajes entre un cliente y un agente para una propiedad.
        /// </summary>
        Task<List<ChatViewModel>> GetChatThread(string clienteId, string agenteId, int propertyId, string currentUserId);

        /// <summary>
        /// Envía un mensaje en el chat.
        /// </summary>
        Task SendMessage(SaveChatViewModel vm, string senderId);

        /// <summary>
        /// Obtiene todos los hilos de chat de un usuario.
        /// </summary>
        Task<List<ChatViewModel>> GetUserChats(string userId);
    }
}
