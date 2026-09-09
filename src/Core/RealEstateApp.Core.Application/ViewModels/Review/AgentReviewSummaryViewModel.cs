namespace RealEstateApp.Core.Application.ViewModels.Review
{
    public class AgentReviewSummaryViewModel
    {
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
    }
}
