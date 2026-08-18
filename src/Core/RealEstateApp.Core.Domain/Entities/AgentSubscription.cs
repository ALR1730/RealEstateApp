using System;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Registro de la suscripción activa o histórica de un agente.
    /// </summary>
    public class AgentSubscription : AuditableBaseEntity
    {
        public string AgentId { get; set; } = string.Empty;
        
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan? SubscriptionPlan { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool AutoRenew { get; set; } = true;
    }
}
