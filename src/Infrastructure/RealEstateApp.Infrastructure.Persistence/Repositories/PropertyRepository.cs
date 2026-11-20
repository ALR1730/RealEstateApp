using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.DTOs.Common;
using RealEstateApp.Core.Application.DTOs.Dashboard;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repositorio de propiedades con carga de relaciones públicas y paginación acotada.
    /// </summary>
    public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
    {
        public PropertyRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        /// <summary>
        /// Obtiene hasta un máximo acotado de propiedades recientes con sus relaciones públicas.
        /// </summary>
        public override async Task<List<Property>> GetAllAsync()
        {
            return await IncludePublicRelations(_dbContext.Set<Property>())
                .OrderByDescending(p => p.Created)
                .Take(PagedResult<Property>.MaxPageSize)
                .ToListAsync();
        }

        public async Task<PagedResult<Property>> GetPagedAsync(
            int page = PagedResult<Property>.DefaultPage,
            int pageSize = PagedResult<Property>.DefaultPageSize)
        {
            var (normalizedPage, normalizedSize) = PagedResult<Property>.Normalize(page, pageSize);

            var totalCount = await _dbContext.Set<Property>()
                .AsNoTracking()
                .CountAsync();

            var items = await IncludePublicRelations(_dbContext.Set<Property>())
                .OrderByDescending(p => p.Created)
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToListAsync();

            return PagedResult<Property>.Create(items, normalizedPage, normalizedSize, totalCount);
        }

        /// <summary>
        /// Obtiene una propiedad pública por ID sin cargar colecciones privadas (Offers ni Chats).
        /// </summary>
        public override async Task<Property?> GetByIdAsync(int id)
        {
            return await IncludePublicRelations(_dbContext.Set<Property>())
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Property>> GetWithFiltersAsync(PropertyFilterViewModel filters)
        {
            var paged = await GetWithFiltersPagedAsync(filters, includeSold: true);
            return paged.Items.ToList();
        }

        /// <summary>
        /// Obtiene propiedades paginadas aplicando filtros combinados directamente en la base de datos.
        /// </summary>
        public async Task<PagedResult<Property>> GetWithFiltersPagedAsync(PropertyFilterViewModel filters, bool includeSold = true)
        {
            filters ??= new PropertyFilterViewModel();
            var (page, pageSize) = PagedResult<Property>.Normalize(filters.Page ?? filters.PageNumber, filters.PageSize);

            IQueryable<Property> query = _dbContext.Set<Property>().AsNoTracking();
            query = ApplyIdentityAndLocationFilters(query, filters, includeSold);
            query = ApplyNumericRangeFilters(query, filters);
            query = await ApplyFeatureFiltersAsync(query, filters);

            var currentUtc = DateTime.UtcNow;
            var orderedQuery = query
                .OrderByDescending(p => p.IsFeatured && (!p.FeaturedUntil.HasValue || p.FeaturedUntil > currentUtc))
                .ThenByDescending(p => p.Created);

            if (HasGeoFilter(filters))
            {
                return await ExecuteGeoFilteredPagedAsync(IncludePublicRelations(orderedQuery), filters, page, pageSize);
            }

            var totalCount = await orderedQuery.CountAsync();
            var items = await IncludePublicRelations(orderedQuery)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return PagedResult<Property>.Create(items, page, pageSize, totalCount);
        }

        private static IQueryable<Property> IncludePublicRelations(IQueryable<Property> source)
        {
            return source
                .AsNoTracking()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .AsSplitQuery();
        }

        private static IQueryable<Property> ApplyIdentityAndLocationFilters(
            IQueryable<Property> query,
            PropertyFilterViewModel filters,
            bool includeSold)
        {
            if (!includeSold)
                query = query.Where(p => p.Status != PropertyStatus.Sold);

            if (!string.IsNullOrWhiteSpace(filters.Code))
                query = query.Where(p => p.Code == filters.Code);

            if (filters.PropertyTypeId.HasValue)
                query = query.Where(p => p.PropertyTypeId == filters.PropertyTypeId.Value);

            if (filters.SaleTypeId.HasValue)
                query = query.Where(p => p.SaleTypeId == filters.SaleTypeId.Value);

            if (filters.ProvinceId.HasValue)
                query = query.Where(p => p.ProvinceId == filters.ProvinceId.Value);

            if (filters.MunicipalityId.HasValue)
                query = query.Where(p => p.MunicipalityId == filters.MunicipalityId.Value);

            if (!string.IsNullOrWhiteSpace(filters.Sector))
                query = query.Where(p => p.Sector != null && p.Sector.Contains(filters.Sector));

            if (!string.IsNullOrWhiteSpace(filters.AgentId))
                query = query.Where(p => p.AgentId == filters.AgentId);

            return query;
        }

        private static IQueryable<Property> ApplyNumericRangeFilters(
            IQueryable<Property> query,
            PropertyFilterViewModel filters)
        {
            if (filters.MinPrice.HasValue)
                query = query.Where(p => p.PriceInDOP >= filters.MinPrice.Value);

            if (filters.MaxPrice.HasValue)
                query = query.Where(p => p.PriceInDOP <= filters.MaxPrice.Value);

            if (filters.MinRooms.HasValue)
                query = query.Where(p => p.Rooms >= filters.MinRooms.Value);

            if (filters.MaxRooms.HasValue)
                query = query.Where(p => p.Rooms <= filters.MaxRooms.Value);

            if (filters.MinBathrooms.HasValue)
                query = query.Where(p => p.Bathrooms >= filters.MinBathrooms.Value);

            if (filters.MaxBathrooms.HasValue)
                query = query.Where(p => p.Bathrooms <= filters.MaxBathrooms.Value);

            if (filters.MinSizeInMeters.HasValue)
                query = query.Where(p => p.SizeInMeters >= filters.MinSizeInMeters.Value);

            if (filters.MaxSizeInMeters.HasValue)
                query = query.Where(p => p.SizeInMeters <= filters.MaxSizeInMeters.Value);

            return query;
        }

        private async Task<IQueryable<Property>> ApplyFeatureFiltersAsync(
            IQueryable<Property> query,
            PropertyFilterViewModel filters)
        {
            if (filters.OnlyFinanciable == true)
                query = query.Where(p => p.IsFinanciable);

            if (filters.OnlyWithVirtualTour == true)
            {
                query = query.Where(p =>
                    (p.MatterportModelId != null && p.MatterportModelId != "") ||
                    (p.Tour360Url != null && p.Tour360Url != "") ||
                    (p.VideoUrl != null && p.VideoUrl != ""));
            }

            if (filters.ImprovementIds != null && filters.ImprovementIds.Count > 0)
            {
                query = query.Where(p => p.PropertyImprovements != null &&
                    p.PropertyImprovements.Any(pi => filters.ImprovementIds.Contains(pi.ImprovementId)));
            }

            if (filters.OnlyFeatured == true)
            {
                var now = DateTime.UtcNow;
                query = query.Where(p => p.IsFeatured && (!p.FeaturedUntil.HasValue || p.FeaturedUntil > now));
            }

            if (filters.OnlyVerifiedAgents == true)
            {
                var verifiedAgentIds = await _dbContext.AgentVerifications
                    .Where(v => v.Status == VerificationStatus.Approved)
                    .Select(v => v.AgentId)
                    .ToListAsync();

                query = query.Where(p => verifiedAgentIds.Contains(p.AgentId));
            }

            return query;
        }

        private static bool HasGeoFilter(PropertyFilterViewModel filters)
        {
            return filters.UserLat.HasValue
                && filters.UserLng.HasValue
                && filters.MaxDistanceKm.HasValue
                && filters.MaxDistanceKm.Value > 0;
        }

        private static async Task<PagedResult<Property>> ExecuteGeoFilteredPagedAsync(
            IQueryable<Property> query,
            PropertyFilterViewModel filters,
            int page,
            int pageSize)
        {
            var uLat = filters.UserLat!.Value;
            var uLng = filters.UserLng!.Value;
            var maxDist = filters.MaxDistanceKm!.Value;

            // Pre-filtrado SQL por bounding box para acotar candidatos antes de evaluar Haversine.
            var latDelta = maxDist / 111.045;
            var cosLat = Math.Max(0.01, Math.Abs(Math.Cos(uLat * Math.PI / 180.0)));
            var lonDelta = maxDist / (111.045 * cosLat);

            var candidates = await query
                .Where(p =>
                    p.Latitude != 0 && p.Longitude != 0 &&
                    p.Latitude >= uLat - latDelta && p.Latitude <= uLat + latDelta &&
                    p.Longitude >= uLng - lonDelta && p.Longitude <= uLng + lonDelta)
                .ToListAsync();

            var filtered = candidates
                .Where(p => CalculateDistanceKm(uLat, uLng, p.Latitude, p.Longitude) <= maxDist)
                .ToList();

            var totalCount = filtered.Count;
            var pagedItems = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return PagedResult<Property>.Create(pagedItems, page, pageSize, totalCount);
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
            return await IncludePublicRelations(_dbContext.Set<Property>())
                .Where(p => p.AgentId == agentId)
                .OrderByDescending(p => p.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una propiedad por su código único de 6 dígitos mediante consulta a base de datos.
        /// </summary>
        public async Task<Property?> GetByCodeAsync(string code)
        {
            return await IncludePublicRelations(_dbContext.Set<Property>())
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

        public async Task<List<Property>> GetCandidateComparablesAsync(int targetPropertyId, int? municipalityId, int? provinceId)
        {
            var query = _dbContext.Set<Property>()
                .AsNoTracking()
                .Include(p => p.Municipality)
                .Where(p => p.Id != targetPropertyId && p.Status != RealEstateApp.Core.Domain.Constants.PropertyStatus.Sold);

            if (municipalityId.HasValue)
            {
                var localCandidates = await query
                    .Where(p => p.MunicipalityId == municipalityId.Value || (provinceId.HasValue && p.ProvinceId == provinceId.Value))
                    .ToListAsync();

                if (localCandidates.Count > 0)
                    return localCandidates;
            }
            else if (provinceId.HasValue)
            {
                var localCandidates = await query
                    .Where(p => p.ProvinceId == provinceId.Value)
                    .ToListAsync();

                if (localCandidates.Count > 0)
                    return localCandidates;
            }

            return await query
                .Where(p => p.Latitude != 0 || p.Longitude != 0)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene métricas consolidadas de propiedades mediante consultas agregadas directas en base de datos.
        /// Evita la carga de entidades en memoria y el overhead de serialización/deserialización.
        /// </summary>
        public async Task<PropertyDashboardMetricsDto> GetDashboardMetricsAsync()
        {
            var statusCounts = await _dbContext.Set<Property>()
                .AsNoTracking()
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var available = statusCounts.FirstOrDefault(x => x.Status == PropertyStatus.Available)?.Count ?? 0;
            var reserved = statusCounts.FirstOrDefault(x => x.Status == PropertyStatus.Reserved)?.Count ?? 0;
            var sold = statusCounts.FirstOrDefault(x => x.Status == PropertyStatus.Sold)?.Count ?? 0;
            var total = statusCounts.Sum(x => x.Count);

            var typeCounts = await _dbContext.Set<Property>()
                .AsNoTracking()
                .GroupBy(p => p.PropertyType != null ? p.PropertyType.Name : "Sin Categoría")
                .Select(g => new PropertyTypeCountDto
                {
                    TypeName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return new PropertyDashboardMetricsDto
            {
                TotalAvailableProperties = available,
                TotalReservedProperties = reserved,
                TotalSoldProperties = sold,
                TotalProperties = total,
                PropertiesByType = typeCounts
            };
        }

        public async Task<string?> GetAgentIdByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<Property>()
                .AsNoTracking()
                .Where(p => p.Id == propertyId)
                .Select(p => p.AgentId)
                .FirstOrDefaultAsync();
        }

        public async Task<Property?> FindByIdAsync(int id)
        {
            return await _dbContext.Set<Property>()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
