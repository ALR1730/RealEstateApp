using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class PriceRangeExtractor : IAiQueryEntityExtractor
    {
        private const string RangePattern =
            @"\b(?:entre|between)\s+(?:rd\$|\$)?\s*([0-9.,]{1,20})\s*(millones?|mil|million|millions|m|k)?\s+(?:y|and)\s+(?:rd\$|\$)?\s*([0-9.,]{1,20})\s*(millones?|mil|million|millions|m|k)?\b";

        private const string MaxPattern =
            @"\b(?:menos\s+de|hasta|m[aá]ximo(?:\s+de)?|por\s+debajo\s+de|menor\s+a|under|below|less\s+than|up\s+to|max(?:\s+of)?)\s+(?:rd\$|\$)?\s*([0-9.,]{1,20})\s*(millones?|mil|million|millions|m|k)?\b";

        private const string MinPattern =
            @"\b(?:m[aá]s\s+de|desde|m[ií]nimo(?:\s+de)?|mayor\s+a|a\s+partir\s+de|more\s+than|over|above|from|starting\s+at|min(?:\s+of)?)\s+(?:rd\$|\$)?\s*([0-9.,]{1,20})\s*(millones?|mil|million|millions|m|k)?\b";

        public void Extract(AiExtractionContext context)
        {
            if (TryExtractRange(context))
            {
                return;
            }

            if (TryExtractMax(context))
            {
                return;
            }

            TryExtractMin(context);
        }

        private static bool TryExtractRange(AiExtractionContext context)
        {
            var match = AiSearchTextHelper.SafeMatch(context.NormalizedQuery, RangePattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return false;
            }

            var p1 = ParsePriceNumber(match.Groups[1].Value, match.Groups[2].Value);
            var p2 = ParsePriceNumber(match.Groups[3].Value, match.Groups[4].Value);
            if (!p1.HasValue || !p2.HasValue)
            {
                return false;
            }

            context.Filter.MinPrice = Math.Min(p1.Value, p2.Value);
            context.Filter.MaxPrice = Math.Max(p1.Value, p2.Value);
            context.Entities.MinPrice = context.Filter.MinPrice;
            context.Entities.MaxPrice = context.Filter.MaxPrice;
            context.RecordSignal();
            return true;
        }

        private static bool TryExtractMax(AiExtractionContext context)
        {
            var match = AiSearchTextHelper.SafeMatch(context.NormalizedQuery, MaxPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return false;
            }

            var max = ParsePriceNumber(match.Groups[1].Value, match.Groups[2].Value);
            if (!max.HasValue)
            {
                return false;
            }

            context.Filter.MaxPrice = max.Value;
            context.Entities.MaxPrice = max.Value;
            context.RecordSignal();
            return true;
        }

        private static void TryExtractMin(AiExtractionContext context)
        {
            var match = AiSearchTextHelper.SafeMatch(context.NormalizedQuery, MinPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return;
            }

            var min = ParsePriceNumber(match.Groups[1].Value, match.Groups[2].Value);
            if (!min.HasValue)
            {
                return;
            }

            context.Filter.MinPrice = min.Value;
            context.Entities.MinPrice = min.Value;
            context.RecordSignal();
        }

        private static decimal? ParsePriceNumber(string numStr, string? unitStr)
        {
            if (string.IsNullOrWhiteSpace(numStr))
            {
                return null;
            }

            var cleanNumber = numStr.Trim().Replace(",", ".");
            if (!decimal.TryParse(cleanNumber, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
            {
                return null;
            }

            var unit = unitStr?.Trim().ToLowerInvariant();
            if (unit is "millon" or "millones" or "million" or "millions" or "m")
            {
                return val * 1_000_000m;
            }

            if (unit is "mil" or "thousand" or "k")
            {
                return val * 1_000m;
            }

            if (val is > 0 and <= 100 && string.IsNullOrEmpty(unit))
            {
                return val * 1_000_000m;
            }

            return val;
        }
    }
}
