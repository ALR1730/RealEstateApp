using System.Collections.Generic;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Provincia de República Dominicana (31 provincias + Distrito Nacional).
    /// Código ISO 3166-2:DO para estandarización geográfica.
    /// </summary>
    public class Province : AuditableBaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty; // Ej: "DO-01"
        
        public ICollection<Municipality>? Municipalities { get; set; }
        public ICollection<Property>? Properties { get; set; }
    }
}
