using System;

namespace RealEstateApp.Core.Application.ViewModels.Chat
{
    /// <summary>
    /// ViewModel de lectura para mostrar mensajes de chat.
    /// </summary>
    public class ChatViewModel
    {
        public int Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteName { get; set; } = string.Empty;
        public string AgenteId { get; set; } = string.Empty;
        public string AgenteName { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string PropertyCode { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }

        /// <summary>
        /// Indica si el mensaje fue enviado por el usuario actual.
        /// </summary>
        public bool IsMine { get; set; }
    }
}
