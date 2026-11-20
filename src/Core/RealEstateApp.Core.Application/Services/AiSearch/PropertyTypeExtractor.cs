using System;
using System.Collections.Generic;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class PropertyTypeExtractor : IAiQueryEntityExtractor
    {
        private static readonly IReadOnlyDictionary<string, string[]> Synonyms =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Apartamento"] = new[] { "apartamento", "apartamentos", "apto", "aptos", "departamento", "departamentos", "penthouse", "estudio", "loft", "apartment", "apartments", "condo", "condos", "studio" },
                ["Casa"] = new[] { "casa", "casas", "residencia", "residencias", "townhouse", "vivienda", "house", "houses", "home", "homes" },
                ["Villa"] = new[] { "villa", "villas", "quinta", "chalet" },
                ["Solar"] = new[] { "solar", "solares", "terreno", "terrenos", "parcela", "lote", "lot", "lots", "land" },
                ["Comercial"] = new[] { "local", "locales", "oficina", "oficinas", "nave", "comercial", "edificio", "office", "offices", "commercial" }
            };

        public void Extract(AiExtractionContext context)
        {
            foreach (var type in context.Catalog.PropertyTypes)
            {
                var normalizedName = AiSearchTextHelper.NormalizeText(type.Name);
                var matched = AiSearchTextHelper.MatchesWord(context.NormalizedQuery, normalizedName)
                    || AiSearchTextHelper.MatchesCatalogSynonyms(context.NormalizedQuery, normalizedName, Synonyms);

                if (!matched)
                {
                    continue;
                }

                context.Filter.PropertyTypeId = type.Id;
                context.Entities.PropertyType = type.Name;
                context.RecordSignal();
                return;
            }
        }
    }
}
