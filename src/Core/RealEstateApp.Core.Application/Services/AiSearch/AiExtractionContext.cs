using RealEstateApp.Core.Application.DTOs.AiSearch;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class AiExtractionContext
    {
        public AiExtractionContext(string normalizedQuery, AiSearchCatalogSnapshot catalog)
        {
            NormalizedQuery = normalizedQuery;
            Catalog = catalog;
        }

        public string NormalizedQuery { get; }
        public AiSearchCatalogSnapshot Catalog { get; }
        public PropertyFilterViewModel Filter { get; } = new();
        public AiExtractedEntitiesDto Entities { get; } = new();
        public int MatchedSignals { get; private set; }

        public void RecordSignal()
        {
            MatchedSignals++;
        }
    }
}
