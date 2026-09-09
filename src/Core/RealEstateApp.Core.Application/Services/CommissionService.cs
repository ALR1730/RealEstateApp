using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;

        public CommissionService(
            ICommissionRepository commissionRepository,
            IOfferRepository offerRepository,
            IPropertyRepository propertyRepository,
            ISubscriptionService subscriptionService,
            UserManager<IdentityUser> userManager)
        {
            _commissionRepository = commissionRepository;
            _offerRepository = offerRepository;
            _propertyRepository = propertyRepository;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
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

            var commission = new Commission
            {
                AgentId = property.AgentId,
                PropertyId = property.Id,
                OfferId = offer.Id,
                SalePrice = offer.MontoOfertado,
                Rate = rate,
                Amount = Math.Round(offer.MontoOfertado * rate / 100m, 2),
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

            foreach (var vm in result)
            {
                var agent = await _userManager.FindByIdAsync(vm.AgentId);
                if (agent != null)
                {
                    var claims = await _userManager.GetClaimsAsync(agent);
                    var firstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
                    var lastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value;
                    var fullName = $"{firstName} {lastName}".Trim();
                    vm.AgentName = !string.IsNullOrEmpty(fullName)
                        ? fullName
                        : (agent.UserName ?? agent.Email);
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