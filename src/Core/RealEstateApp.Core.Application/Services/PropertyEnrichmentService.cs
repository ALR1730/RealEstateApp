using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio especializado en cálculos de conversión de moneda, formateo de precios,
    /// enriquecimiento de datos de agentes y análisis de rebajas de precios.
    /// </summary>
    public class PropertyEnrichmentService : IPropertyEnrichmentService
    {
        private readonly ICurrencyService _currencyService;
        private readonly IAccountService _accountService;
        private readonly IPropertyPriceHistoryRepository _priceHistoryRepository;
        private readonly ILogger<PropertyEnrichmentService> _logger;

        public PropertyEnrichmentService(
            ICurrencyService currencyService,
            IAccountService accountService,
            IPropertyPriceHistoryRepository priceHistoryRepository,
            ILogger<PropertyEnrichmentService>? logger = null)
        {
            _currencyService = currencyService;
            _accountService = accountService;
            _priceHistoryRepository = priceHistoryRepository;
            _logger = logger ?? NullLogger<PropertyEnrichmentService>.Instance;
        }

        public async Task EnrichPropertiesWithCurrencyAsync(List<PropertyViewModel> viewModels)
        {
            if (viewModels == null || viewModels.Count == 0) return;
            var exchangeRate = await _currencyService.GetExchangeRateAsync();

            foreach (var vm in viewModels)
            {
                if (string.IsNullOrEmpty(vm.Currency))
                {
                    vm.Currency = CurrencyConstants.DOP;
                }

                if (string.Equals(vm.Currency, CurrencyConstants.USD, StringComparison.OrdinalIgnoreCase))
                {
                    vm.PriceInUSD = vm.Price;
                    vm.PriceInDOP = vm.Price * exchangeRate;
                }
                else
                {
                    vm.PriceInDOP = vm.Price;
                    vm.PriceInUSD = exchangeRate > 0 ? Math.Round(vm.Price / exchangeRate, 2) : vm.Price;
                }

                vm.DisplayPrice = _currencyService.FormatPrice(vm.Price, vm.Currency);
                vm.DisplaySecondaryPrice = _currencyService.FormatSecondaryPrice(vm.Price, vm.Currency, vm.Currency, exchangeRate);

                await EnrichPriceDropAsync(vm);
            }
        }

        public async Task EnrichPriceDropAsync(PropertyViewModel vm)
        {
            if (vm == null || vm.HasPriceDrop) return;

            try
            {
                var latestHistory = await _priceHistoryRepository.GetLatestByPropertyIdAsync(vm.Id);
                if (latestHistory != null && latestHistory.OldPrice > 0 && latestHistory.NewPrice < latestHistory.OldPrice)
                {
                    vm.HasPriceDrop = true;
                    vm.PriceDropPercentage = Math.Abs(latestHistory.PercentageChange);
                    vm.PriceDropAmount = latestHistory.OldPrice - latestHistory.NewPrice;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al consultar historial de precio reciente para la propiedad {PropertyId}", vm.Id);
            }
        }

        public async Task EnrichAgentNamesAsync(List<PropertyViewModel> viewModels)
        {
            if (viewModels == null || viewModels.Count == 0) return;

            var agentIds = viewModels
                .Where(v => !string.IsNullOrWhiteSpace(v.AgentId))
                .Select(v => v.AgentId)
                .Distinct()
                .ToList();

            if (agentIds.Count == 0) return;

            Dictionary<string, AccountUserDto>? userDict = null;
            try
            {
                userDict = await _accountService.GetUsersByIdsAsync(agentIds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al consultar nombres de agentes para {Count} identificadores", agentIds.Count);
                return;
            }

            if (userDict == null || userDict.Count == 0) return;

            foreach (var vm in viewModels)
            {
                if (!string.IsNullOrWhiteSpace(vm.AgentId) &&
                    userDict.TryGetValue(vm.AgentId, out var agent))
                {
                    vm.AgentName = $"{agent.FirstName} {agent.LastName}".Trim();
                }
            }
        }
    }
}
