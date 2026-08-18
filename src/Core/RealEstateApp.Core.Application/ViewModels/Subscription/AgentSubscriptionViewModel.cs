using System;
using System.Collections.Generic;

namespace RealEstateApp.Core.Application.ViewModels.Subscription
{
    public class AgentSubscriptionViewModel
    {
        public int Id { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public int SubscriptionPlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int MaxActiveProperties { get; set; }
        public int MaxFeaturedProperties { get; set; }
        public bool Allows3DTours { get; set; }
        public bool AllowsVideo { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool AutoRenew { get; set; }
    }

    public class AgentSubscriptionDashboardViewModel
    {
        public AgentSubscriptionViewModel? CurrentSubscription { get; set; }
        public List<SubscriptionPlanViewModel> AvailablePlans { get; set; } = new();
        public int CurrentActivePropertiesCount { get; set; }
        public int MaxAllowedProperties { get; set; }
        public bool CanCreateMoreProperties => CurrentActivePropertiesCount < MaxAllowedProperties;
    }
}
