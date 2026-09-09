namespace RealEstateApp.Core.Application.ViewModels.Commission
{
    public class CommissionSummaryViewModel
    {
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
        public int PaidCount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal AverageRate { get; set; }
    }
}