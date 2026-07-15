using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Entidad join explícita para la relación muchos-a-muchos entre Property e Improvement.
    /// </summary>
    public class PropertyImprovement
    {
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public int ImprovementId { get; set; }
        public Improvement? Improvement { get; set; }
    }
}
