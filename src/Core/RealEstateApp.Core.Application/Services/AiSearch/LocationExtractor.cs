namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public sealed class LocationExtractor : IAiQueryEntityExtractor
    {
        private static readonly string[] CommonSectors =
        {
            "Bella Vista", "Piantini", "Naco", "Evaristo Morales", "Gazcue", "Arroyo Hondo",
            "Mirador Sur", "Mirador Norte", "Los Cacicazgos", "Ensanche Paraíso", "Anacaona",
            "Alma Rosa", "Ensanche Ozama", "Gurabo", "Punta Cana", "Bávaro", "Cap Cana",
            "Las Terrenas", "Sosúa", "Cabarete", "Jardines Metropolitanos", "La Julia"
        };

        public void Extract(AiExtractionContext context)
        {
            foreach (var province in context.Catalog.Provinces)
            {
                var normalizedProvince = AiSearchTextHelper.NormalizeText(province.Name);
                if (!AiSearchTextHelper.MatchesWord(context.NormalizedQuery, normalizedProvince))
                {
                    continue;
                }

                context.Filter.ProvinceId = province.Id;
                context.Entities.Province = province.Name;
                context.RecordSignal();
                break;
            }

            foreach (var sector in CommonSectors)
            {
                var normalizedSector = AiSearchTextHelper.NormalizeText(sector);
                if (!AiSearchTextHelper.MatchesWord(context.NormalizedQuery, normalizedSector))
                {
                    continue;
                }

                context.Filter.Sector = sector;
                context.Entities.Sector = sector;
                context.RecordSignal();
                break;
            }
        }
    }
}
