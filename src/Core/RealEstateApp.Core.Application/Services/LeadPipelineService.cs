using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.LeadPipeline;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    public class LeadPipelineService : ILeadPipelineService
    {
        private readonly ILeadPipelineRepository _leadRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public LeadPipelineService(
            ILeadPipelineRepository leadRepository,
            IPropertyRepository propertyRepository,
            IMapper mapper)
        {
            _leadRepository = leadRepository;
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<List<LeadPipelineDto>> GetAllByAgentAsync(string agentId)
        {
            var leads = await _leadRepository.GetByAgentIdAsync(agentId);
            return _mapper.Map<List<LeadPipelineDto>>(leads);
        }

        public async Task<List<LeadPipelineDto>> GetByStageAsync(string agentId, string stage)
        {
            var leads = await _leadRepository.GetByAgentAndStageAsync(agentId, stage);
            return _mapper.Map<List<LeadPipelineDto>>(leads);
        }

        public async Task<LeadPipelineDto?> GetByIdAsync(int id)
        {
            var lead = await _leadRepository.GetByIdWithPropertyAsync(id);
            if (lead == null)
                throw new NotFoundException("Lead no encontrado.");

            return _mapper.Map<LeadPipelineDto>(lead);
        }

        public async Task<LeadPipelineDto> CreateAsync(CreateLeadRequest request, string agentId)
        {
            var existingLeads = await _leadRepository.GetByAgentIdAsync(agentId);
            var maxSortOrder = existingLeads.Any() ? existingLeads.Max(l => l.SortOrder) : 0;

            var lead = new LeadPipeline
            {
                LeadName = request.LeadName,
                LeadEmail = request.LeadEmail,
                LeadPhone = request.LeadPhone,
                Notes = request.Notes,
                AgentId = agentId,
                Stage = PipelineStage.NewLead,
                Priority = request.Priority ?? "Normal",
                Source = request.Source,
                EstimatedBudget = request.EstimatedBudget,
                BudgetCurrency = request.BudgetCurrency,
                PropertyId = request.PropertyId,
                SortOrder = maxSortOrder + 1,
                Created = DateTime.Now
            };

            await _leadRepository.AddAsync(lead);

            return _mapper.Map<LeadPipelineDto>(lead);
        }

        public async Task<LeadPipelineDto> UpdateAsync(int id, UpdateLeadRequest request)
        {
            var lead = await _leadRepository.GetByIdWithPropertyAsync(id);
            if (lead == null)
                throw new NotFoundException("Lead no encontrado.");

            if (request.LeadName != null)
                lead.LeadName = request.LeadName;
            if (request.LeadEmail != null)
                lead.LeadEmail = request.LeadEmail;
            if (request.LeadPhone != null)
                lead.LeadPhone = request.LeadPhone;
            if (request.Notes != null)
                lead.Notes = request.Notes;
            if (request.Priority != null)
                lead.Priority = request.Priority!;
            if (request.Source != null)
                lead.Source = request.Source;
            if (request.EstimatedBudget != null)
                lead.EstimatedBudget = request.EstimatedBudget;
            if (request.BudgetCurrency != null)
                lead.BudgetCurrency = request.BudgetCurrency;
            if (request.PropertyId != null)
                lead.PropertyId = request.PropertyId;
            if (request.NextFollowUpDate != null)
                lead.NextFollowUpDate = request.NextFollowUpDate.Value;

            await _leadRepository.UpdateAsync(lead);

            return _mapper.Map<LeadPipelineDto>(lead);
        }

        public async Task<LeadPipelineDto> MoveToStageAsync(int id, string newStage)
        {
            var validStages = new[]
            {
                PipelineStage.NewLead,
                PipelineStage.Contacted,
                PipelineStage.Visit,
                PipelineStage.Offer,
                PipelineStage.Closing,
                PipelineStage.Won,
                PipelineStage.Lost
            };

            if (!validStages.Contains(newStage))
                throw new ValidationException($"La etapa '{newStage}' no es una etapa válida del pipeline.");

            var lead = await _leadRepository.GetByIdWithPropertyAsync(id);
            if (lead == null)
                throw new NotFoundException("Lead no encontrado.");

            lead.Stage = newStage;

            await _leadRepository.UpdateAsync(lead);

            return _mapper.Map<LeadPipelineDto>(lead);
        }

        public async Task<LeadPipelineDto> UpdateSortOrderAsync(int id, int newSortOrder)
        {
            var lead = await _leadRepository.GetByIdWithPropertyAsync(id);
            if (lead == null)
                throw new NotFoundException("Lead no encontrado.");

            lead.SortOrder = newSortOrder;

            await _leadRepository.UpdateAsync(lead);

            return _mapper.Map<LeadPipelineDto>(lead);
        }

        public async Task DeleteAsync(int id)
        {
            var lead = await _leadRepository.GetByIdWithPropertyAsync(id);
            if (lead == null)
                throw new NotFoundException("Lead no encontrado.");

            await _leadRepository.DeleteAsync(lead);
        }

        public async Task<LeadPipelineStatsDto> GetStatsAsync(string agentId)
        {
            var leads = await _leadRepository.GetByAgentIdAsync(agentId);

            var totalLeads = leads.Count;
            var newLeadCount = leads.Count(l => l.Stage == PipelineStage.NewLead);
            var contactedCount = leads.Count(l => l.Stage == PipelineStage.Contacted);
            var visitCount = leads.Count(l => l.Stage == PipelineStage.Visit);
            var offerCount = leads.Count(l => l.Stage == PipelineStage.Offer);
            var closingCount = leads.Count(l => l.Stage == PipelineStage.Closing);
            var wonCount = leads.Count(l => l.Stage == PipelineStage.Won);
            var lostCount = leads.Count(l => l.Stage == PipelineStage.Lost);

            var conversionRate = totalLeads > 0
                ? (decimal)wonCount / totalLeads * 100
                : 0;

            var totalPipelineValue = leads
                .Where(l => l.Stage == PipelineStage.Offer || l.Stage == PipelineStage.Closing)
                .Sum(l => l.EstimatedBudget ?? 0);

            var today = DateTime.Today;
            var leadsNeedingFollowUp = leads
                .Where(l => l.NextFollowUpDate.HasValue && l.NextFollowUpDate.Value.Date <= today)
                .ToList();

            return new LeadPipelineStatsDto
            {
                TotalLeads = totalLeads,
                NewLeadCount = newLeadCount,
                ContactedCount = contactedCount,
                VisitCount = visitCount,
                OfferCount = offerCount,
                ClosingCount = closingCount,
                WonCount = wonCount,
                LostCount = lostCount,
                ConversionRate = Math.Round(conversionRate, 2),
                TotalPipelineValue = totalPipelineValue,
                LeadsNeedingFollowUp = _mapper.Map<List<LeadPipelineDto>>(leadsNeedingFollowUp)
            };
        }
    }
}
