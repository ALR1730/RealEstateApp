using System;

namespace RealEstateApp.Core.Application.ViewModels.Commission
{
    public class CommissionViewModel
    {
        public int Id { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public string? AgentName { get; set; }
        public int PropertyId { get; set; }
        public string? PropertyCode { get; set; }
        public string? PropertyName { get; set; }
        public int OfferId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}