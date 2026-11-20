using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class SaleTypeExtractor : IAiQueryEntityExtractor
    {
        private const string RentPattern = @"\b(alquiler|renta|alquilar|en renta|arrendar|rent|rental|for rent|lease)\b";
        private const string SalePattern = @"\b(venta|comprar|en venta|adquirir|compra|sale|buy|for sale|purchase)\b";

        public void Extract(AiExtractionContext context)
        {
            if (TryAssignMatchingSaleType(context, RentPattern, name =>
                    name.Contains("Alquiler", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Renta", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            TryAssignMatchingSaleType(context, SalePattern, name =>
                name.Contains("Venta", StringComparison.OrdinalIgnoreCase));
        }

        private static bool TryAssignMatchingSaleType(
            AiExtractionContext context,
            string pattern,
            Func<string, bool> namePredicate)
        {
            if (!AiSearchTextHelper.SafeIsMatch(context.NormalizedQuery, pattern, RegexOptions.IgnoreCase))
            {
                return false;
            }

            var match = context.Catalog.SaleTypes.FirstOrDefault(s => namePredicate(s.Name));
            if (match == null)
            {
                return false;
            }

            context.Filter.SaleTypeId = match.Id;
            context.Entities.SaleType = match.Name;
            context.RecordSignal();
            return true;
        }
    }
}
