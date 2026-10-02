using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Commission;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Sistema de comisiones por venta para agentes (Ítem 2.3).
    /// La comisión se genera automáticamente al aceptar una oferta,
    /// usando la tasa configurada en el plan del agente (o el default global).
    /// </summary>
    public class CommissionService : ICommissionService
    {
        private readonly ICommissionRepository _commissionRepository;
        private readonly IOfferRepository _offerRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAccountService _accountService;

        public CommissionService(
            ICommissionRepository commissionRepository,
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository,
            ISubscriptionService subscriptionService,
            IAccountService accountService)
        {
            _commissionRepository = commissionRepository;
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
            _subscriptionService = subscriptionService;
            _accountService = accountService;
        }

        public async Task CreateForAcceptedOfferAsync(int offerId)
        {
            var offer = await _offerRepository.GetByIdAsync(offerId);
            if (offer == null)
                throw new NotFoundException("La oferta no existe");

            var property = await _propertyRepository.GetByIdAsync(offer.PropertyId);
            if (property == null)
                throw new NotFoundException("La propiedad no existe");

            // Evitar comisiones duplicadas para la misma oferta
            var existing = await _commissionRepository.GetAllAsync();
            if (existing.Any(c => c.OfferId == offerId))
                return;

            var rate = await ResolveCommissionRateAsync(property.AgentId);

            // Usar el monto de cierre real: si existe una contraoferta aceptada, ese es el precio pactado.
            // De lo contrario, usar el monto original ofertado por el cliente.
            decimal finalSalePrice = (offer.CounterOfferAmount.HasValue && offer.CounterOfferAmount.Value > 0)
                ? offer.CounterOfferAmount.Value
                : offer.MontoOfertado;

            var commission = new Commission
            {
                AgentId = property.AgentId,
                PropertyId = property.Id,
                OfferId = offer.Id,
                SalePrice = finalSalePrice,
                Rate = rate,
                Amount = Math.Round(finalSalePrice * rate / 100m, 2),
                Status = CommissionStatus.Pending
            };

            await _commissionRepository.AddAsync(commission);
        }

        /// <summary>
        /// Tasa de comisión del agente: la del plan activo si está configurada,
        /// de lo contrario el default global.
        /// </summary>
        private async Task<decimal> ResolveCommissionRateAsync(string agentId)
        {
            var currentSub = await _subscriptionService.GetCurrentSubscriptionByAgentIdAsync(agentId);
            var planRate = currentSub?.CommissionPercentage;

            return planRate.HasValue && planRate.Value > 0
                ? planRate.Value
                : CommissionConstants.DefaultRate;
        }

        public async Task<List<CommissionViewModel>> GetByAgentAsync(string agentId, bool fetchDetails = true)
        {
            var commissions = await _commissionRepository.GetByAgentIdAsync(agentId);
            return MapToViewModel(commissions);
        }

        public async Task<CommissionSummaryViewModel> GetAgentSummaryAsync(string agentId)
        {
            var commissions = await _commissionRepository.GetByAgentIdAsync(agentId);

            return new CommissionSummaryViewModel
            {
                TotalCount = commissions.Count,
                TotalAmount = commissions.Sum(c => c.Amount),
                PendingCount = commissions.Count(c => c.Status == CommissionStatus.Pending),
                PendingAmount = commissions.Where(c => c.Status == CommissionStatus.Pending).Sum(c => c.Amount),
                PaidCount = commissions.Count(c => c.Status == CommissionStatus.Paid),
                PaidAmount = commissions.Where(c => c.Status == CommissionStatus.Paid).Sum(c => c.Amount),
                AverageRate = commissions.Count > 0 ? Math.Round(commissions.Average(c => c.Rate), 2) : 0
            };
        }

        public async Task<List<CommissionViewModel>> GetAllAsync()
        {
            var commissions = await _commissionRepository.GetAllWithDetailsAsync();
            var result = MapToViewModel(commissions);

            // Batch lookup: 1 sola query para todos los agentes, eliminando el N+1
            var agentIds = result.Select(vm => vm.AgentId).Distinct();
            var agentDict = await _accountService.GetUsersByIdsAsync(agentIds);

            foreach (var vm in result)
            {
                if (agentDict.TryGetValue(vm.AgentId, out var agent))
                {
                    var fullName = $"{agent.FirstName} {agent.LastName}".Trim();
                    vm.AgentName = !string.IsNullOrEmpty(fullName) ? fullName : (agent.UserName ?? agent.Email);
                }
            }

            return result;
        }

        public async Task MarkAsPaidAsync(int commissionId)
        {
            var commission = await _commissionRepository.GetByIdAsync(commissionId);
            if (commission == null)
                throw new NotFoundException("La comisión no existe");

            commission.Status = CommissionStatus.Paid;
            await _commissionRepository.UpdateAsync(commission);
        }

        private List<CommissionViewModel> MapToViewModel(List<Commission> commissions)
        {
            return commissions.Select(c => new CommissionViewModel
            {
                Id = c.Id,
                AgentId = c.AgentId,
                PropertyId = c.PropertyId,
                PropertyCode = c.Property?.Code,
                PropertyName = c.Property?.Name,
                OfferId = c.OfferId,
                SalePrice = c.SalePrice,
                Rate = c.Rate,
                Amount = c.Amount,
                Status = c.Status,
                Created = c.Created
            }).ToList();
        }
    }
}
