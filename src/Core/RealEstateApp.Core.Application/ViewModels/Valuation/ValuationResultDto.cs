using System;
using System.Collections.Generic;

namespace RealEstateApp.Core.Application.ViewModels.Valuation
{
    public class ValuationResultDto
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyCode { get; set; } = string.Empty;

        public decimal CurrentPrice { get; set; }
        public string CurrentCurrency { get; set; } = "DOP";

        public decimal EstimatedPricePerSqm { get; set; }
        public decimal EstimatedTotalPrice { get; set; }
        public decimal PropertySizeSqm { get; set; }

        public int ComparableCount { get; set; }
        public decimal MinPricePerSqm { get; set; }
        public decimal MaxPricePerSqm { get; set; }
        public decimal AveragePricePerSqm { get; set; }
        public decimal StandardDeviation { get; set; }

        public decimal PriceDifference { get; set; }
        public decimal PriceDifferencePercentage { get; set; }
        public string ValuationRating { get; set; } = string.Empty; // Sobrevalorada, Justa, Subvalorada

        public int ConfidenceScore { get; set; }
        public decimal SearchRadiusKm { get; set; }

        public List<ComparablePropertyDto> Comparables { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ComparablePropertyDto
    {
        public int PropertyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "DOP";
        public decimal SizeInMeters { get; set; }
        public decimal PricePerSqm { get; set; }
        public string? Sector { get; set; }
        public string? MunicipalityName { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
    }
}
