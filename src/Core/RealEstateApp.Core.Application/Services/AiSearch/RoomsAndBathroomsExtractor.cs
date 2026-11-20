using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class RoomsAndBathroomsExtractor : IAiQueryEntityExtractor
    {
        private const string RoomsUnitPattern = @"(?:habitaci[oó]n(?:es)?|hab(?:s)?|cuartos?|dormitorios?|bedrooms?|beds?|rooms?)";
        private const string BathroomsUnitPattern = @"(?:ba[ñn]os?|bathrooms?|baths?)";

        private static readonly IReadOnlyDictionary<string, int> RoomWords = new Dictionary<string, int>
        {
            ["un"] = 1, ["una"] = 1, ["uno"] = 1, ["one"] = 1,
            ["dos"] = 2, ["two"] = 2,
            ["tres"] = 3, ["three"] = 3,
            ["cuatro"] = 4, ["four"] = 4,
            ["cinco"] = 5, ["five"] = 5,
            ["seis"] = 6, ["six"] = 6
        };

        private static readonly IReadOnlyDictionary<string, int> BathroomWords = new Dictionary<string, int>
        {
            ["un"] = 1, ["uno"] = 1, ["one"] = 1,
            ["dos"] = 2, ["two"] = 2,
            ["tres"] = 3, ["three"] = 3,
            ["cuatro"] = 4, ["four"] = 4
        };

        public void Extract(AiExtractionContext context)
        {
            var rooms = ExtractQuantity(context.NormalizedQuery, RoomsUnitPattern, RoomWords);
            if (rooms.HasValue)
            {
                context.Filter.MinRooms = rooms.Value;
                context.Entities.MinRooms = rooms.Value;
                context.RecordSignal();
            }

            var bathrooms = ExtractQuantity(context.NormalizedQuery, BathroomsUnitPattern, BathroomWords);
            if (bathrooms.HasValue)
            {
                context.Filter.MinBathrooms = bathrooms.Value;
                context.Entities.MinBathrooms = bathrooms.Value;
                context.RecordSignal();
            }
        }

        private static int? ExtractQuantity(
            string text,
            string unitPattern,
            IReadOnlyDictionary<string, int> wordMap)
        {
            var numericPattern = $@"\b(?:con\s+|with\s+)?(\d{{1,2}})\s*(?:\+|\s*o\s*m[aá]s|\s*or\s*more)?\s*{unitPattern}\b";
            var numericMatch = AiSearchTextHelper.SafeMatch(text, numericPattern, RegexOptions.IgnoreCase);
            if (numericMatch.Success && int.TryParse(numericMatch.Groups[1].Value, out var value))
            {
                return value;
            }

            foreach (var entry in wordMap)
            {
                var wordPattern = $@"\b{entry.Key}\s+{unitPattern}\b";
                if (AiSearchTextHelper.SafeIsMatch(text, wordPattern, RegexOptions.IgnoreCase))
                {
                    return entry.Value;
                }
            }

            return null;
        }
    }
}
