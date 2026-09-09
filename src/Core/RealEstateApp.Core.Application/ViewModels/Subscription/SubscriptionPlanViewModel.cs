namespace RealEstateApp.Core.Application.ViewModels.Subscription
{
    public class SubscriptionPlanViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int MaxActiveProperties { get; set; }
        public int MaxFeaturedProperties { get; set; }
        public bool Allows3DTours { get; set; }
        public bool AllowsVideo { get; set; }
        public decimal? CommissionPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}
