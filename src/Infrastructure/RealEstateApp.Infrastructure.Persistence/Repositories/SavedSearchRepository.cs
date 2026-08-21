using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class SavedSearchRepository : GenericRepository<SavedSearch>, ISavedSearchRepository
    {
        public SavedSearchRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<SavedSearch>> GetByUserIdAsync(string userId)
        {
            return await _dbContext.Set<SavedSearch>()
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .Include(s => s.PropertyType)
                .Include(s => s.SaleType)
                .Include(s => s.Province)
                .Include(s => s.Municipality)
                .OrderByDescending(s => s.Created)
                .ToListAsync();
        }

        public async Task<List<SavedSearch>> GetMatchingSearchesAsync(Property property)
        {
            // Obtener búsquedas con alertas activadas
            var query = _dbContext.Set<SavedSearch>()
                .AsNoTracking()
                .Where(s => s.EmailAlertsEnabled || s.InAppAlertsEnabled)
                .AsQueryable();

            // Filtrar por Tipo de Propiedad si está definido en la búsqueda
            query = query.Where(s => !s.PropertyTypeId.HasValue || s.PropertyTypeId == property.PropertyTypeId);

            // Filtrar por Modalidad de Venta si está definido
            query = query.Where(s => !s.SaleTypeId.HasValue || s.SaleTypeId == property.SaleTypeId);

            // Filtrar por Provincia si está definido
            query = query.Where(s => !s.ProvinceId.HasValue || s.ProvinceId == property.ProvinceId);

            // Filtrar por Municipio si está definido
            query = query.Where(s => !s.MunicipalityId.HasValue || s.MunicipalityId == property.MunicipalityId);

            // Filtrar por Rango de Precio (usando PriceInDOP estandarizado)
            var priceInDOP = property.PriceInDOP > 0 ? property.PriceInDOP : property.Price;
            query = query.Where(s => !s.MinPrice.HasValue || priceInDOP >= s.MinPrice.Value);
            query = query.Where(s => !s.MaxPrice.HasValue || priceInDOP <= s.MaxPrice.Value);

            // Filtrar por Habitaciones
            query = query.Where(s => !s.MinRooms.HasValue || property.Rooms >= s.MinRooms.Value);
            query = query.Where(s => !s.MaxRooms.HasValue || property.Rooms <= s.MaxRooms.Value);

            // Filtrar por Baños
            query = query.Where(s => !s.MinBathrooms.HasValue || property.Bathrooms >= s.MinBathrooms.Value);
            query = query.Where(s => !s.MaxBathrooms.HasValue || property.Bathrooms <= s.MaxBathrooms.Value);

            // Filtrar por Financiamiento
            query = query.Where(s => !s.OnlyFinanciable.HasValue || !s.OnlyFinanciable.Value || property.IsFinanciable);

            // Filtrar por Tour Virtual
            query = query.Where(s => !s.OnlyWithVirtualTour.HasValue || !s.OnlyWithVirtualTour.Value || 
                (property.Tour360Url != null && property.Tour360Url != "") || 
                (property.MatterportModelId != null && property.MatterportModelId != "") ||
                (property.VideoUrl != null && property.VideoUrl != ""));

            return await query.ToListAsync();
        }
    }
}
