using System.Collections.Generic;
using RealEstateApp.Core.Application.DTOs.AiSearch;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public static class AiExplanationBuilder
    {
        public static string Build(AiExtractedEntitiesDto entities)
        {
            var parts = new List<string>
            {
                !string.IsNullOrEmpty(entities.PropertyType) ? entities.PropertyType : "Inmuebles"
            };

            if (!string.IsNullOrEmpty(entities.SaleType))
                parts.Add($"en {entities.SaleType}");

            if (!string.IsNullOrEmpty(entities.Sector))
                parts.Add($"en el sector {entities.Sector}");
            else if (!string.IsNullOrEmpty(entities.Province))
                parts.Add($"en {entities.Province}");

            if (entities.MinRooms.HasValue)
                parts.Add($"con al menos {entities.MinRooms} hab.");

            if (entities.MinBathrooms.HasValue)
                parts.Add($"y {entities.MinBathrooms} baño(s)");

            AppendPriceClause(parts, entities);

            if (entities.Improvements.Count > 0)
                parts.Add($"que incluyan {string.Join(", ", entities.Improvements)}");

            if (entities.HasVirtualTour == true)
                parts.Add("con tour virtual 360°");

            return "Buscando " + string.Join(" ", parts) + ".";
        }

        private static void AppendPriceClause(List<string> parts, AiExtractedEntitiesDto entities)
        {
            if (entities.MinPrice.HasValue && entities.MaxPrice.HasValue)
                parts.Add($"con precio entre RD$ {entities.MinPrice:N0} y RD$ {entities.MaxPrice:N0}");
            else if (entities.MaxPrice.HasValue)
                parts.Add($"con precio hasta RD$ {entities.MaxPrice:N0}");
            else if (entities.MinPrice.HasValue)
                parts.Add($"con precio desde RD$ {entities.MinPrice:N0}");
        }
    }
}
