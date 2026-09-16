using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Valuation;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    public class PropertyValuationService : IPropertyValuationService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyValuationRepository _valuationRepository;
        private readonly ICurrencyService _currencyService;
        private readonly IMapper _mapper;

        public PropertyValuationService(
            IPropertyRepository propertyRepository,
            IPropertyValuationRepository valuationRepository,
            ICurrencyService currencyService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _valuationRepository = valuationRepository;
            _currencyService = currencyService;
            _mapper = mapper;
        }

        public async Task<ValuationResultDto> CalculateValuationAsync(int propertyId, decimal searchRadiusKm = 5.0m)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                throw new NotFoundException(nameof(Property), propertyId);

            var allProperties = await _propertyRepository.GetAllAsync();

            var comparables = FindComparables(property, allProperties, searchRadiusKm);

            var pricePerSqmList = comparables
                .Where(p => p.SizeInMeters > 0 && p.PriceInDOP > 0)
                .Select(p => new
                {
                    PricePerSqm = p.PriceInDOP / p.SizeInMeters,
                    DistanceKm = CalculateDistance(
                        property.Latitude, property.Longitude,
                        p.Latitude, p.Longitude),
                    SameSector = string.Equals(
                        (property.Sector ?? string.Empty).Trim(),
                        (p.Sector ?? string.Empty).Trim(),
                        StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            decimal averagePricePerSqm = 0;
            decimal minPricePerSqm = 0;
            decimal maxPricePerSqm = 0;
            decimal standardDeviation = 0;
            decimal estimatedPricePerSqm = 0;

            if (pricePerSqmList.Count > 0)
            {
                minPricePerSqm = pricePerSqmList.Min(x => x.PricePerSqm);
                maxPricePerSqm = pricePerSqmList.Max(x => x.PricePerSqm);
                averagePricePerSqm = pricePerSqmList.Average(x => x.PricePerSqm);

                var variance = pricePerSqmList
                    .Select(x => Math.Pow((double)(x.PricePerSqm - averagePricePerSqm), 2))
                    .Average();
                standardDeviation = (decimal)Math.Sqrt(variance);

                var sectorMatches = pricePerSqmList.Where(x => x.SameSector).ToList();
                if (sectorMatches.Count > 0 && sectorMatches.Count < pricePerSqmList.Count)
                {
                    decimal sectorAvg = sectorMatches.Average(x => x.PricePerSqm);
                    decimal nonSectorAvg = pricePerSqmList
                        .Where(x => !x.SameSector)
                        .Average(x => x.PricePerSqm);
                    int total = pricePerSqmList.Count;
                    decimal sectorWeight = (decimal)sectorMatches.Count / total;
                    estimatedPricePerSqm = (sectorAvg * sectorWeight) + (nonSectorAvg * (1 - sectorWeight));
                }
                else
                {
                    estimatedPricePerSqm = averagePricePerSqm;
                }
            }

            decimal propertySize = property.SizeInMeters;
            decimal estimatedTotalPrice = estimatedPricePerSqm * propertySize;

            var exchangeRate = await _currencyService.GetExchangeRateAsync();
            decimal currentPriceInDOP = _currencyService.ConvertToDOP(property.Price, property.Currency, exchangeRate);

            decimal priceDifference = estimatedTotalPrice - currentPriceInDOP;
            decimal priceDifferencePercentage = currentPriceInDOP > 0
                ? Math.Round((priceDifference / currentPriceInDOP) * 100, 2)
                : 0;

            string valuationRating;
            if (priceDifferencePercentage > 10)
                valuationRating = "Sobrevalorada";
            else if (priceDifferencePercentage < -10)
                valuationRating = "Subvalorada";
            else
                valuationRating = "Justa";

            int confidenceScore = CalculateConfidenceScore(
                pricePerSqmList.Count,
                comparables.Count,
                propertySize,
                comparables,
                searchRadiusKm);

            var comparableDtos = comparables
                .OrderBy(p => CalculateDistance(
                    property.Latitude, property.Longitude,
                    p.Latitude, p.Longitude))
                .Select(p =>
                {
                    var dto = _mapper.Map<ComparablePropertyDto>(p);
                    dto.PropertyId = p.Id;
                    dto.PricePerSqm = p.SizeInMeters > 0 && p.PriceInDOP > 0
                        ? Math.Round(p.PriceInDOP / p.SizeInMeters, 2)
                        : 0;
                    dto.DistanceKm = (decimal)Math.Round(CalculateDistance(
                        property.Latitude, property.Longitude,
                        p.Latitude, p.Longitude), 2);
                    dto.MunicipalityName = p.Municipality?.Name ?? string.Empty;
                    return dto;
                })
                .ToList();

            var valuation = new PropertyValuation
            {
                PropertyId = propertyId,
                EstimatedPricePerSqm = Math.Round(estimatedPricePerSqm, 2),
                EstimatedTotalPrice = Math.Round(estimatedTotalPrice, 2),
                ComparableCount = pricePerSqmList.Count,
                MinPricePerSqm = Math.Round(minPricePerSqm, 2),
                MaxPricePerSqm = Math.Round(maxPricePerSqm, 2),
                StandardDeviation = Math.Round(standardDeviation, 2),
                SearchRadiusKm = searchRadiusKm,
                Currency = CurrencyConstants.DOP,
                ConfidenceScore = confidenceScore,
                LastUpdated = DateTime.UtcNow
            };

            await _valuationRepository.AddAsync(valuation);

            return new ValuationResultDto
            {
                PropertyId = property.Id,
                PropertyName = property.Name,
                PropertyCode = property.Code,
                CurrentPrice = property.Price,
                CurrentCurrency = property.Currency,
                EstimatedPricePerSqm = Math.Round(estimatedPricePerSqm, 2),
                EstimatedTotalPrice = Math.Round(estimatedTotalPrice, 2),
                PropertySizeSqm = propertySize,
                ComparableCount = pricePerSqmList.Count,
                MinPricePerSqm = Math.Round(minPricePerSqm, 2),
                MaxPricePerSqm = Math.Round(maxPricePerSqm, 2),
                AveragePricePerSqm = Math.Round(averagePricePerSqm, 2),
                StandardDeviation = Math.Round(standardDeviation, 2),
                PriceDifference = Math.Round(priceDifference, 2),
                PriceDifferencePercentage = priceDifferencePercentage,
                ValuationRating = valuationRating,
                ConfidenceScore = confidenceScore,
                SearchRadiusKm = searchRadiusKm,
                Comparables = comparableDtos,
                CalculatedAt = DateTime.UtcNow
            };
        }

        public async Task<ValuationResultDto?> GetLastValuationAsync(int propertyId)
        {
            var valuation = await _valuationRepository.GetByPropertyIdAsync(propertyId);
            if (valuation == null)
                return null;

            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                return null;

            var exchangeRate = await _currencyService.GetExchangeRateAsync();
            decimal currentPriceInDOP = _currencyService.ConvertToDOP(property.Price, property.Currency, exchangeRate);

            decimal priceDifference = valuation.EstimatedTotalPrice - currentPriceInDOP;
            decimal priceDifferencePercentage = currentPriceInDOP > 0
                ? Math.Round((priceDifference / currentPriceInDOP) * 100, 2)
                : 0;

            string valuationRating;
            if (priceDifferencePercentage > 10)
                valuationRating = "Sobrevalorada";
            else if (priceDifferencePercentage < -10)
                valuationRating = "Subvalorada";
            else
                valuationRating = "Justa";

            var comparables = FindComparables(property, await _propertyRepository.GetAllAsync(), valuation.SearchRadiusKm);

            var comparableDtos = comparables
                .OrderBy(p => CalculateDistance(
                    property.Latitude, property.Longitude,
                    p.Latitude, p.Longitude))
                .Select(p =>
                {
                    var dto = _mapper.Map<ComparablePropertyDto>(p);
                    dto.PropertyId = p.Id;
                    dto.PricePerSqm = p.SizeInMeters > 0 && p.PriceInDOP > 0
                        ? Math.Round(p.PriceInDOP / p.SizeInMeters, 2)
                        : 0;
                    dto.DistanceKm = (decimal)Math.Round(CalculateDistance(
                        property.Latitude, property.Longitude,
                        p.Latitude, p.Longitude), 2);
                    dto.MunicipalityName = p.Municipality?.Name ?? string.Empty;
                    return dto;
                })
                .ToList();

            return new ValuationResultDto
            {
                PropertyId = property.Id,
                PropertyName = property.Name,
                PropertyCode = property.Code,
                CurrentPrice = property.Price,
                CurrentCurrency = property.Currency,
                EstimatedPricePerSqm = valuation.EstimatedPricePerSqm,
                EstimatedTotalPrice = valuation.EstimatedTotalPrice,
                PropertySizeSqm = property.SizeInMeters,
                ComparableCount = valuation.ComparableCount,
                MinPricePerSqm = valuation.MinPricePerSqm,
                MaxPricePerSqm = valuation.MaxPricePerSqm,
                AveragePricePerSqm = valuation.ComparableCount > 0
                    ? Math.Round((valuation.MinPricePerSqm + valuation.MaxPricePerSqm) / 2, 2)
                    : 0,
                StandardDeviation = valuation.StandardDeviation,
                PriceDifference = Math.Round(priceDifference, 2),
                PriceDifferencePercentage = priceDifferencePercentage,
                ValuationRating = valuationRating,
                ConfidenceScore = valuation.ConfidenceScore,
                SearchRadiusKm = valuation.SearchRadiusKm,
                Comparables = comparableDtos,
                CalculatedAt = valuation.LastUpdated
            };
        }

        private static List<Property> FindComparables(Property target, List<Property> allProperties, decimal searchRadiusKm)
        {
            var candidates = allProperties
                .Where(p => p.Id != target.Id)
                .Where(p => p.Status != PropertyStatus.Sold)
                .ToList();

            bool hasMunicipality = target.MunicipalityId.HasValue;
            bool hasProvince = target.ProvinceId.HasValue;

            var sameMunicipality = candidates
                .Where(p => hasMunicipality && p.MunicipalityId == target.MunicipalityId)
                .ToList();

            if (sameMunicipality.Count > 0)
                return sameMunicipality;

            var sameProvince = candidates
                .Where(p => hasProvince && p.ProvinceId == target.ProvinceId)
                .ToList();

            if (sameProvince.Count > 0)
                return sameProvince;

            return candidates
                .Where(p => CalculateDistance(
                    target.Latitude, target.Longitude,
                    p.Latitude, p.Longitude) <= (double)searchRadiusKm)
                .ToList();
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * (Math.PI / 180.0);
            double dLon = (lon2 - lon1) * (Math.PI / 180.0);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) *
                       Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371.0 * c;
        }

        private static int CalculateConfidenceScore(
            int pricedComparableCount,
            int totalComparableCount,
            decimal propertySize,
            List<Property> comparables,
            decimal searchRadiusKm)
        {
            int score = 0;

            if (pricedComparableCount >= 10)
                score += 40;
            else if (pricedComparableCount >= 5)
                score += 30;
            else if (pricedComparableCount >= 3)
                score += 20;
            else if (pricedComparableCount >= 1)
                score += 10;

            var sizes = comparables
                .Where(p => p.SizeInMeters > 0)
                .Select(p => p.SizeInMeters)
                .ToList();

            if (sizes.Count > 0 && propertySize > 0)
            {
                decimal avgSize = sizes.Average();
                decimal sizeDeviation = avgSize > 0
                    ? Math.Abs(propertySize - avgSize) / avgSize
                    : 1;

                if (sizeDeviation < 0.1m)
                    score += 30;
                else if (sizeDeviation < 0.25m)
                    score += 20;
                else if (sizeDeviation < 0.5m)
                    score += 10;
            }

            if (searchRadiusKm <= 2)
                score += 20;
            else if (searchRadiusKm <= 5)
                score += 15;
            else
                score += 10;

            return Math.Min(score, 100);
        }
    }
}
