using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repositorio de propiedades con carga de relaciones (Eager Loading).
    /// </summary>
    public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
    {
        public PropertyRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <summary>
        /// Obtiene todas las propiedades con sus relaciones cargadas.
        /// </summary>
        public override async Task<List<Property>> GetAllAsync()
        {
            return await _dbContext.Set<Property>()
                .AsNoTracking()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .AsSplitQuery()
                .OrderByDescending(p => p.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una propiedad por ID con todas sus relaciones.
        /// </summary>
        public override async Task<Property?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Property>()
                .AsNoTracking()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .Include(p => p.Offers)
                .Include(p => p.Chats)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtiene propiedades aplicando filtros combinados directamente en la base de datos (IQueryable).
        /// </summary>
        public async Task<List<Property>> GetWithFiltersAsync(RealEstateApp.Core.Application.ViewModels.Property.PropertyFilterViewModel filters)
        {
            var query = _dbContext.Set<Property>()
                .AsNoTracking()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Code))
            {
                query = query.Where(p => p.Code == filters.Code);
            }

            if (filters.PropertyTypeId.HasValue)
            {
                query = query.Where(p => p.PropertyTypeId == filters.PropertyTypeId.Value);
            }

            if (filters.SaleTypeId.HasValue)
            {
                query = query.Where(p => p.SaleTypeId == filters.SaleTypeId.Value);
            }

            if (filters.ProvinceId.HasValue)
            {
                query = query.Where(p => p.ProvinceId == filters.ProvinceId.Value);
            }

            if (filters.MunicipalityId.HasValue)
            {
                query = query.Where(p => p.MunicipalityId == filters.MunicipalityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Sector))
            {
                query = query.Where(p => p.Sector != null && p.Sector.Contains(filters.Sector));
            }

            if (filters.MinPrice.HasValue)
            {
                query = query.Where(p => p.PriceInDOP >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(p => p.PriceInDOP <= filters.MaxPrice.Value);
            }

            if (filters.MinRooms.HasValue)
            {
                query = query.Where(p => p.Rooms >= filters.MinRooms.Value);
            }

            if (filters.MaxRooms.HasValue)
            {
                query = query.Where(p => p.Rooms <= filters.MaxRooms.Value);
            }

            if (filters.MinBathrooms.HasValue)
            {
                query = query.Where(p => p.Bathrooms >= filters.MinBathrooms.Value);
            }

            if (filters.MaxBathrooms.HasValue)
            {
                query = query.Where(p => p.Bathrooms <= filters.MaxBathrooms.Value);
            }

            if (filters.MinSizeInMeters.HasValue)
            {
                query = query.Where(p => p.SizeInMeters >= filters.MinSizeInMeters.Value);
            }

            if (filters.MaxSizeInMeters.HasValue)
            {
                query = query.Where(p => p.SizeInMeters <= filters.MaxSizeInMeters.Value);
            }

            if (filters.OnlyFinanciable == true)
            {
                query = query.Where(p => p.IsFinanciable);
            }

            if (filters.OnlyWithVirtualTour == true)
            {
                query = query.Where(p => 
                    (p.MatterportModelId != null && p.MatterportModelId != "") ||
                    (p.Tour360Url != null && p.Tour360Url != "") ||
                    (p.VideoUrl != null && p.VideoUrl != "")
                );
            }

            if (filters.ImprovementIds != null && filters.ImprovementIds.Count > 0)
            {
                query = query.Where(p => p.PropertyImprovements != null && 
                    p.PropertyImprovements.Any(pi => filters.ImprovementIds.Contains(pi.ImprovementId)));
            }

            if (!string.IsNullOrWhiteSpace(filters.AgentId))
            {
                query = query.Where(p => p.AgentId == filters.AgentId);
            }

            if (filters.OnlyFeatured == true)
            {
                var now = DateTime.UtcNow;
                query = query.Where(p => p.IsFeatured && (!p.FeaturedUntil.HasValue || p.FeaturedUntil > now));
            }

            if (filters.OnlyVerifiedAgents == true)
            {
                var verifiedAgentIds = await _dbContext.AgentVerifications
                    .Where(v => v.Status == RealEstateApp.Core.Domain.Constants.VerificationStatus.Approved)
                    .Select(v => v.AgentId)
                    .ToListAsync();

                query = query.Where(p => verifiedAgentIds.Contains(p.AgentId));
            }

            var currentUtc = DateTime.UtcNow;
            query = query
                .OrderByDescending(p => p.IsFeatured && (!p.FeaturedUntil.HasValue || p.FeaturedUntil > currentUtc))
                .ThenByDescending(p => p.Created);

            if (filters.PageNumber.HasValue && filters.PageSize.HasValue)
            {
                var pageNumber = filters.PageNumber.Value < 1 ? 1 : filters.PageNumber.Value;
                var pageSize = filters.PageSize.Value < 1 ? 10 : filters.PageSize.Value;
                query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var results = await query.ToListAsync();

            if (filters.UserLat.HasValue && filters.UserLng.HasValue && filters.MaxDistanceKm.HasValue && filters.MaxDistanceKm.Value > 0)
            {
                var uLat = filters.UserLat.Value;
                var uLng = filters.UserLng.Value;
                var maxDist = filters.MaxDistanceKm.Value;
                results = results.Where(p => p.Latitude != 0 && p.Longitude != 0 && CalculateDistanceKm(uLat, uLng, p.Latitude, p.Longitude) <= maxDist).ToList();
            }

            return results;
        }

        private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double r = 6371.0; // Radio de la Tierra en km
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return r * c;
        }

        /// <summary>
        /// Obtiene todas las propiedades asociadas a un agente específico filtrando a nivel de base de datos.
        /// </summary>
        public async Task<List<Property>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<Property>()
                .AsNoTracking()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .AsSplitQuery()
                .Where(p => p.AgentId == agentId)
                .OrderByDescending(p => p.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una propiedad por su código único de 6 dígitos mediante consulta a base de datos.
        /// </summary>
        public async Task<Property?> GetByCodeAsync(string code)
        {
            return await _dbContext.Set<Property>()
                .AsNoTracking()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        /// <summary>
        /// Obtiene todos los sectores únicos registrados en las propiedades de la base de datos,
        /// opcionalmente filtrados por provincia y/o municipio.
        /// </summary>
        public async Task<List<string>> GetDistinctSectorsAsync(int? provinceId = null, int? municipalityId = null)
        {
            var query = _dbContext.Set<Property>()
                .AsNoTracking()
                .Where(p => !string.IsNullOrEmpty(p.Sector));

            if (provinceId.HasValue && provinceId.Value > 0)
            {
                query = query.Where(p => p.ProvinceId == provinceId.Value);
            }

            if (municipalityId.HasValue && municipalityId.Value > 0)
            {
                query = query.Where(p => p.MunicipalityId == municipalityId.Value);
            }

            return await query
                .Select(p => p.Sector!.Trim())
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public override async Task<Property> AddAsync(Property entity)
        {
            SyncPriceInDOP(entity);
            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Property entity)
        {
            SyncPriceInDOP(entity);
            await base.UpdateAsync(entity);
        }

        private void SyncPriceInDOP(Property entity)
        {
            if (string.IsNullOrEmpty(entity.Currency))
            {
                entity.Currency = RealEstateApp.Core.Domain.Constants.CurrencyConstants.DOP;
            }

            if (string.Equals(entity.Currency, RealEstateApp.Core.Domain.Constants.CurrencyConstants.USD, System.StringComparison.OrdinalIgnoreCase))
            {
                entity.PriceInDOP = entity.Price * RealEstateApp.Core.Domain.Constants.CurrencyConstants.DefaultUsdToDopRate;
            }
            else
            {
                entity.PriceInDOP = entity.Price;
            }
        }
    }
}
