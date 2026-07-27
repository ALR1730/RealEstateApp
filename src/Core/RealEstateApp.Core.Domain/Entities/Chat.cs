using System;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Chat : AuditableBaseEntity
    {
        public string ClienteId { get; set; } = string.Empty;
        public string AgenteId { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public string MessageContent { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty; // Indica quién envió (ClienteId o AgenteId)
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsWhatsApp { get; set; } = true;
        public string? WhatsAppMessageId { get; set; }
    }
}
