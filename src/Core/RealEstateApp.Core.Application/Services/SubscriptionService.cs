using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Subscription;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IAgentSubscriptionRepository _agentSubscriptionRepository;
        private readonly IPropertyRepository _propertyRepository;

        public SubscriptionService(
            ISubscriptionPlanRepository planRepository,
            IAgentSubscriptionRepository agentSubscriptionRepository,
            IPropertyRepository propertyRepository)
        {
            _planRepository = planRepository;
            _agentSubscriptionRepository = agentSubscriptionRepository;
            _propertyRepository = propertyRepository;
        }

        public async Task<List<SubscriptionPlanViewModel>> GetAvailablePlansAsync()
        {
            var plans = await _planRepository.GetActivePlansAsync();
            return plans.Select(p => new SubscriptionPlanViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                MonthlyPrice = p.MonthlyPrice,
                MaxActiveProperties = p.MaxActiveProperties,
                MaxFeaturedProperties = p.MaxFeaturedProperties,
                Allows3DTours = p.Allows3DTours,
                AllowsVideo = p.AllowsVideo,
                IsActive = p.IsActive
            }).ToList();
        }

        public async Task<AgentSubscriptionViewModel?> GetCurrentSubscriptionByAgentIdAsync(string agentId)
        {
            var sub = await _agentSubscriptionRepository.GetActiveByAgentIdAsync(agentId);
            if (sub == null)
            {
                // Si no tiene suscripción explícita, se le asocia el plan gratuito por defecto
                var freePlan = await _planRepository.GetDefaultFreePlanAsync();
                if (freePlan != null)
                {
                    return new AgentSubscriptionViewModel
                    {
                        AgentId = agentId,
                        SubscriptionPlanId = freePlan.Id,
                        PlanName = freePlan.Name,
                        MonthlyPrice = freePlan.MonthlyPrice,
                        MaxActiveProperties = freePlan.MaxActiveProperties,
                        MaxFeaturedProperties = freePlan.MaxFeaturedProperties,
                        Allows3DTours = freePlan.Allows3DTours,
                        AllowsVideo = freePlan.AllowsVideo,
                        StartDate = DateTime.UtcNow,
                        IsActive = true,
                        AutoRenew = true
                    };
                }
                return null;
            }

            var plan = sub.SubscriptionPlan ?? await _planRepository.GetByIdAsync(sub.SubscriptionPlanId);

            return new AgentSubscriptionViewModel
            {
                Id = sub.Id,
                AgentId = sub.AgentId,
                SubscriptionPlanId = sub.SubscriptionPlanId,
                PlanName = plan?.Name ?? "Plan Activo",
                MonthlyPrice = plan?.MonthlyPrice ?? 0,
                MaxActiveProperties = plan?.MaxActiveProperties ?? 3,
                MaxFeaturedProperties = plan?.MaxFeaturedProperties ?? 0,
                Allows3DTours = plan?.Allows3DTours ?? true,
                AllowsVideo = plan?.AllowsVideo ?? true,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                IsActive = sub.IsActive,
                AutoRenew = sub.AutoRenew
            };
        }

        public async Task<AgentSubscriptionDashboardViewModel> GetAgentDashboardViewModelAsync(string agentId)
        {
            var currentSub = await GetCurrentSubscriptionByAgentIdAsync(agentId);
            var plans = await GetAvailablePlansAsync();
            var agentProperties = await _propertyRepository.GetByAgentIdAsync(agentId);

            var activePropsCount = agentProperties.Count;
            var maxAllowed = currentSub?.MaxActiveProperties ?? 3;

            return new AgentSubscriptionDashboardViewModel
            {
                CurrentSubscription = currentSub,
                AvailablePlans = plans,
                CurrentActivePropertiesCount = activePropsCount,
                MaxAllowedProperties = maxAllowed
            };
        }

        public async Task<bool> SubscribeAgentAsync(string agentId, int planId)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null) return false;

            var existing = await _agentSubscriptionRepository.GetActiveByAgentIdAsync(agentId);
            if (existing != null)
            {
                existing.IsActive = false;
                existing.EndDate = DateTime.UtcNow;
                await _agentSubscriptionRepository.UpdateAsync(existing);
            }

            var newSub = new AgentSubscription
            {
                AgentId = agentId,
                SubscriptionPlanId = planId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                AutoRenew = true
            };

            await _agentSubscriptionRepository.AddAsync(newSub);
            return true;
        }

        public async Task<bool> CanAgentCreatePropertyAsync(string agentId)
        {
            var currentSub = await GetCurrentSubscriptionByAgentIdAsync(agentId);
            var maxAllowed = currentSub?.MaxActiveProperties ?? 3;

            var agentProperties = await _propertyRepository.GetByAgentIdAsync(agentId);
            return agentProperties.Count < maxAllowed;
        }

        public async Task<(bool Allowed, string Message)> CanAgentFeaturePropertyAsync(string agentId, int propertyId = 0)
        {
            var currentSub = await GetCurrentSubscriptionByAgentIdAsync(agentId);
            var maxAllowed = currentSub?.MaxFeaturedProperties ?? 0;

            if (maxAllowed <= 0)
            {
                return (false, "Tu plan de suscripción actual no incluye propiedades destacadas. Actualiza a un plan Profesional o Inmobiliaria para poder destacar tus inmuebles.");
            }

            var agentProperties = await _propertyRepository.GetByAgentIdAsync(agentId);
            var currentFeaturedCount = agentProperties.Count(p => p.Id != propertyId && p.IsFeatured && (!p.FeaturedUntil.HasValue || p.FeaturedUntil > DateTime.UtcNow));

            if (currentFeaturedCount >= maxAllowed)
            {
                return (false, $"Has alcanzado el límite de {maxAllowed} propiedad(es) destacada(s) permitidas por tu plan '{currentSub?.PlanName}'. Desactiva otra propiedad o amplía tu suscripción.");
            }

            return (true, "Permitido");
        }
    }
}
