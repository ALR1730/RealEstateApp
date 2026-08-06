using System;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Offer : AuditableBaseEntity
    {
        public decimal MontoOfertado { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;
        public string ClienteId { get; set; } = string.Empty;
        public DateTime FechaOferta { get; set; } = DateTime.UtcNow;
        public string? PreApprovalLetterUrl { get; set; }

        // Campos de Contra-Oferta (Ítem 2.1)
        public decimal? CounterOfferAmount { get; set; }
        public string? CounterOfferMessage { get; set; }
        public DateTime? CounterOfferDate { get; set; }

        public int PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}
