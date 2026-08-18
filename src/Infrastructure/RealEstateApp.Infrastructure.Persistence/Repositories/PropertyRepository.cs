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
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .OrderByDescending(p => p.Created)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una propiedad por ID con todas sus relaciones.
        /// </summary>
        public override async Task<Property?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<Property>()
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
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtiene propiedades aplicando filtros combinados directamente en la base de datos (IQueryable).
        /// </summary>
        public async Task<List<Property>> GetWithFiltersAsync(RealEstateApp.Core.Application.ViewModels.Property.PropertyFilterViewModel filters)
        {
            var query = _dbContext.Set<Property>()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
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
                query = query.Where(p => p.Price >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filters.MaxPrice.Value);
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

            if (!string.IsNullOrWhiteSpace(filters.AgentId))
            {
                query = query.Where(p => p.AgentId == filters.AgentId);
            }

            if (filters.OnlyFeatured == true)
            {
                var now = DateTime.UtcNow;
                query = query.Where(p => p.IsFeatured && (!p.FeaturedUntil.HasValue || p.FeaturedUntil > now));
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

            return await query.ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las propiedades asociadas a un agente específico filtrando a nivel de base de datos.
        /// </summary>
        public async Task<List<Property>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<Property>()
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
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
                .Include(p => p.PropertyType)
                .Include(p => p.SaleType)
                .Include(p => p.Province)
                .Include(p => p.Municipality)
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .FirstOrDefaultAsync(p => p.Code == code);
        }
    }
}
