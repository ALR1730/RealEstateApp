using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Plan de suscripción para agentes inmobiliarios (Gratuito, Pro, Premium).
    /// </summary>
    public class SubscriptionPlan : AuditableBaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int MaxActiveProperties { get; set; }
        public int MaxFeaturedProperties { get; set; }
        public bool Allows3DTours { get; set; } = true;
        public bool AllowsVideo { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public ICollection<AgentSubscription>? Subscriptions { get; set; }
    }
}
