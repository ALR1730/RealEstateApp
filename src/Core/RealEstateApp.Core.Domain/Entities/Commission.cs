using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Registro de comisión del agente tras aceptar una oferta de compra (Ítem 2.3).
    /// Se crea automáticamente cuando se acepta una oferta.
    /// </summary>
    public class Commission : AuditableBaseEntity
    {
        public string AgentId { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public int OfferId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = CommissionStatus.Pending;

        public Property? Property { get; set; }
        public Offer? Offer { get; set; }
    }
}
