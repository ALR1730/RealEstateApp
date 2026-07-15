using System.Collections.Generic;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Improvement : AuditableBaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation properties (M:N via join table)
        public ICollection<PropertyImprovement>? PropertyImprovements { get; set; }
    }
}
