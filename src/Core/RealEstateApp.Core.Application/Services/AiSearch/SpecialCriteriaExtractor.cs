namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class SpecialCriteriaExtractor : IAiQueryEntityExtractor
    {
        private const string VirtualTourPattern = @"\b(virtual|tour\s+virtual|recorrido\s+360|360|matterport)\b";
        private const string FinanciablePattern = @"\b(financiable|con\s+financiamiento|cr[eé]dito|aplica\s+a\s+pr[eé]stamo)\b";
        private const string FeaturedPattern = @"\b(destacado|destacados|destacada|premium|top)\b";

        public void Extract(AiExtractionContext context)
        {
            if (AiSearchTextHelper.SafeIsMatch(context.NormalizedQuery, VirtualTourPattern))
            {
                context.Filter.OnlyWithVirtualTour = true;
                context.Entities.HasVirtualTour = true;
                context.RecordSignal();
            }

            if (AiSearchTextHelper.SafeIsMatch(context.NormalizedQuery, FinanciablePattern))
            {
                context.Filter.OnlyFinanciable = true;
                context.Entities.IsFinanciable = true;
                context.RecordSignal();
            }

            if (AiSearchTextHelper.SafeIsMatch(context.NormalizedQuery, FeaturedPattern))
            {
                context.Filter.OnlyFeatured = true;
                context.Entities.IsFeatured = true;
                context.RecordSignal();
            }
        }
    }
}
