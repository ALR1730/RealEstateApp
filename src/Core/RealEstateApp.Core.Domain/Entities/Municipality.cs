using System.Collections.Generic;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Municipio perteneciente a una provincia de República Dominicana.
    /// </summary>
    public class Municipality : AuditableBaseEntity
    {
        public string Name { get; set; } = string.Empty;
        
        public int ProvinceId { get; set; }
        public Province? Province { get; set; }
        
        public ICollection<Property>? Properties { get; set; }
    }
}
