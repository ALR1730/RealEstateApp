using System;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Entidad que registra el histórico de cambios de precio de una propiedad para auditoría y análisis de tendencias.
    /// </summary>
    public class PropertyPriceHistory : AuditableBaseEntity
    {
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string Currency { get; set; } = CurrencyConstants.DOP;
        
        /// <summary>
        /// Porcentaje de variación relativo al precio anterior (ej. -5.2% o +3.1%).
        /// </summary>
        public decimal PercentageChange { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
        public string? ChangedByUserId { get; set; }
        public string? ChangeReason { get; set; }
    }
}
