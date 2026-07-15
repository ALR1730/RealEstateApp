using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Representa un inmueble marcado como favorito por un cliente.
    /// </summary>
    public class Favorite : AuditableBaseEntity
    {
        public string ClienteId { get; set; } = string.Empty;

        public int PropertyId { get; set; }
        public Property? Property { get; set; }
    }
}
