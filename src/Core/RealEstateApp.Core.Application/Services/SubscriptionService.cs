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
                CommissionPercentage = p.CommissionPercentage,
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
                        CommissionPercentage = freePlan.CommissionPercentage,
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
                CommissionPercentage = plan?.CommissionPercentage,
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

        public async Task<bool> UpdatePlanCommissionPercentageAsync(int planId, decimal percentage)
        {
            if (percentage < 0 || percentage > 100) return false;

            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null) return false;

            plan.CommissionPercentage = percentage;
            await _planRepository.UpdateAsync(plan);
            return true;
        }

        // ==================== Administración de planes (CRUD) ====================

        public async Task<List<SubscriptionPlanViewModel>> GetAllPlansAsync()
        {
            var plans = await _planRepository.GetAllAsync();
            return plans.Select(p => MapToViewModel(p)).OrderBy(p => p.MonthlyPrice).ToList();
        }

        public async Task<SubscriptionPlanViewModel?> GetPlanByIdAsync(int planId)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            return plan == null ? null : MapToViewModel(plan);
        }

        public async Task<SubscriptionPlanViewModel?> CreatePlanAsync(SaveSubscriptionPlanViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name)) return null;

            var plan = new SubscriptionPlan
            {
                Name = model.Name.Trim(),
                Description = model.Description ?? string.Empty,
                MonthlyPrice = model.MonthlyPrice,
                MaxActiveProperties = model.MaxActiveProperties,
                MaxFeaturedProperties = model.MaxFeaturedProperties,
                Allows3DTours = model.Allows3DTours,
                AllowsVideo = model.AllowsVideo,
                CommissionPercentage = model.CommissionPercentage,
                IsActive = model.IsActive
            };

            await _planRepository.AddAsync(plan);
            return MapToViewModel(plan);
        }

        public async Task<SubscriptionPlanViewModel?> UpdatePlanAsync(int planId, SaveSubscriptionPlanViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name)) return null;

            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null) return null;

            plan.Name = model.Name.Trim();
            plan.Description = model.Description ?? string.Empty;
            plan.MonthlyPrice = model.MonthlyPrice;
            plan.MaxActiveProperties = model.MaxActiveProperties;
            plan.MaxFeaturedProperties = model.MaxFeaturedProperties;
            plan.Allows3DTours = model.Allows3DTours;
            plan.AllowsVideo = model.AllowsVideo;
            plan.CommissionPercentage = model.CommissionPercentage;
            plan.IsActive = model.IsActive;

            await _planRepository.UpdateAsync(plan);
            return MapToViewModel(plan);
        }

        public async Task<bool> SetPlanActiveAsync(int planId, bool isActive)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null) return false;

            plan.IsActive = isActive;
            await _planRepository.UpdateAsync(plan);
            return true;
        }

        public async Task<bool> DeletePlanAsync(int planId)
        {
            var plan = await _planRepository.GetByIdAsync(planId);
            if (plan == null) return false;

            // No se permite eliminar un plan que ya tiene agentes suscritos (integridad referencial).
            var subscriptions = await _agentSubscriptionRepository.GetAllAsync();
            if (subscriptions.Any(s => s.SubscriptionPlanId == planId)) return false;

            await _planRepository.DeleteAsync(plan);
            return true;
        }

        private SubscriptionPlanViewModel MapToViewModel(SubscriptionPlan plan) => new SubscriptionPlanViewModel
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            MonthlyPrice = plan.MonthlyPrice,
            MaxActiveProperties = plan.MaxActiveProperties,
            MaxFeaturedProperties = plan.MaxFeaturedProperties,
            Allows3DTours = plan.Allows3DTours,
            AllowsVideo = plan.AllowsVideo,
            CommissionPercentage = plan.CommissionPercentage,
            IsActive = plan.IsActive
        };
    }
}
