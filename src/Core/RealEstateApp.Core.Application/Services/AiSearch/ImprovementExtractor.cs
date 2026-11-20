using System;
using System.Collections.Generic;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class ImprovementExtractor : IAiQueryEntityExtractor
    {
        private static readonly IReadOnlyDictionary<string, string[]> Synonyms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Balcón"] = new[] { "balcon", "balcones", "terraza", "balcony", "terrace" },
                ["Piscina"] = new[] { "piscina", "alberca", "pileta", "pool", "swimming pool" },
                ["Gimnasio"] = new[] { "gimnasio", "gym", "fitness" },
                ["Ascensor"] = new[] { "ascensor", "elevador", "elevator", "lift" },
                ["Planta Eléctrica"] = new[] { "planta", "planta electrica", "generador", "generator", "power plant" },
                ["Seguridad"] = new[] { "seguridad", "vigilancia", "guardian", "security", "guard" },
                ["Jacuzzi"] = new[] { "jacuzzi", "hidromasaje", "hot tub" },
                ["Parqueo"] = new[] { "parqueo", "parqueos", "estacionamiento", "garaje", "cochera", "parking", "garage" },
                ["Patio"] = new[] { "patio", "jardin", "patio trasero", "yard", "backyard", "garden" }
            };

        public void Extract(AiExtractionContext context)
        {
            var matchedIds = new List<int>();

            foreach (var improvement in context.Catalog.Improvements)
            {
                var normalizedName = AiSearchTextHelper.NormalizeText(improvement.Name);
                var matched = AiSearchTextHelper.MatchesWord(context.NormalizedQuery, normalizedName)
                    || AiSearchTextHelper.MatchesCatalogSynonyms(context.NormalizedQuery, normalizedName, Synonyms);

                if (!matched)
                {
                    continue;
                }

                matchedIds.Add(improvement.Id);
                context.Entities.Improvements.Add(improvement.Name);
                context.RecordSignal();
            }

            if (matchedIds.Count > 0)
            {
                context.Filter.ImprovementIds = matchedIds;
            }
        }
    }
}
