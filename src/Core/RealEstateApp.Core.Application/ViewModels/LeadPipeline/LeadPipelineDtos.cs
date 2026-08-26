using System;
using System.Collections.Generic;

namespace RealEstateApp.Core.Application.ViewModels.LeadPipeline
{
    public class LeadPipelineDto
    {
        public int Id { get; set; }
        public string LeadName { get; set; } = string.Empty;
        public string? LeadEmail { get; set; }
        public string? LeadPhone { get; set; }
        public string? Notes { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public string? Source { get; set; }
        public decimal? EstimatedBudget { get; set; }
        public string? BudgetCurrency { get; set; }
        public int? PropertyId { get; set; }
        public string? PropertyName { get; set; }
        public DateTime? LastContactDate { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int SortOrder { get; set; }
        public DateTime Created { get; set; }
    }

    public class CreateLeadRequest
    {
        public string LeadName { get; set; } = string.Empty;
        public string? LeadEmail { get; set; }
        public string? LeadPhone { get; set; }
        public string? Notes { get; set; }
        public string? Priority { get; set; }
        public string? Source { get; set; }
        public decimal? EstimatedBudget { get; set; }
        public string? BudgetCurrency { get; set; }
        public int? PropertyId { get; set; }
    }

    public class UpdateLeadRequest
    {
        public string? LeadName { get; set; }
        public string? LeadEmail { get; set; }
        public string? LeadPhone { get; set; }
        public string? Notes { get; set; }
        public string? Priority { get; set; }
        public string? Source { get; set; }
        public decimal? EstimatedBudget { get; set; }
        public string? BudgetCurrency { get; set; }
        public int? PropertyId { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
    }

    public class LeadPipelineStatsDto
    {
        public int TotalLeads { get; set; }
        public int NewLeadCount { get; set; }
        public int ContactedCount { get; set; }
        public int VisitCount { get; set; }
        public int OfferCount { get; set; }
        public int ClosingCount { get; set; }
        public int WonCount { get; set; }
        public int LostCount { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public List<LeadPipelineDto> LeadsNeedingFollowUp { get; set; } = new();
    }
}
