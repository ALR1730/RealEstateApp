using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    public class MortgageSimulation : AuditableBaseEntity
    {
        public decimal MontoInicialAportado { get; set; }
        public decimal TasaInteresAnual { get; set; }
        
        public int PropertyId { get; set; }
        public Property? Property { get; set; }
        
        public string ClienteId { get; set; } = string.Empty;
    }
}
