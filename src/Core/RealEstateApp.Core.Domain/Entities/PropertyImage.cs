using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class PropertyImage : AuditableBaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}
