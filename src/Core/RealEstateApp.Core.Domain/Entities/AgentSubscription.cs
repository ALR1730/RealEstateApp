using System;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Exceptions;

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
        public DateTime? EndDate { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool AutoRenew { get; private set; } = true;

        #region Domain Behaviors & Invariants

        public static AgentSubscription CreateActive(
            string agentId,
            int subscriptionPlanId,
            int durationMonths = 1,
            bool autoRenew = true)
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                throw new DomainException("El identificador del agente es obligatorio.");
            }

            if (subscriptionPlanId <= 0)
            {
                throw new DomainException("El plan de suscripción debe ser válido.");
            }

            if (durationMonths <= 0)
            {
                throw new DomainException("La duración de la suscripción debe ser de al menos 1 mes.");
            }

            var start = DateTime.UtcNow;
            return new AgentSubscription
            {
                AgentId = agentId,
                SubscriptionPlanId = subscriptionPlanId,
                StartDate = start,
                EndDate = start.AddMonths(durationMonths),
                IsActive = true,
                AutoRenew = autoRenew
            };
        }

        public void Cancel(DateTime? cancelledAtUtc = null)
        {
            if (!IsActive)
            {
                throw new DomainException("La suscripción ya se encuentra inactiva.");
            }

            IsActive = false;
            AutoRenew = false;
            EndDate = cancelledAtUtc ?? DateTime.UtcNow;
        }

        public void Renew(int additionalMonths = 1)
        {
            if (additionalMonths <= 0)
            {
                throw new DomainException("La cantidad de meses de renovación debe ser mayor a cero.");
            }

            var now = DateTime.UtcNow;
            var baseline = EndDate.HasValue && EndDate.Value > now ? EndDate.Value : now;
            EndDate = baseline.AddMonths(additionalMonths);
            IsActive = true;
        }

        public bool IsExpired(DateTime? referenceUtc = null)
        {
            if (!IsActive)
            {
                return true;
            }

            if (!EndDate.HasValue)
            {
                return false;
            }

            var reference = referenceUtc ?? DateTime.UtcNow;
            return EndDate.Value <= reference;
        }

        #endregion
    }
}
