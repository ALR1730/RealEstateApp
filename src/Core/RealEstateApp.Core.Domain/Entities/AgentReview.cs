using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Reseña y calificación (1-5 estrellas) entregada por un cliente
    /// a un agente tras completar una transacción (Ítem 2.9).
    /// </summary>
    public class AgentReview : AuditableBaseEntity
    {
        public string AgentId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;

        public Property? Property { get; set; }
    }
}
