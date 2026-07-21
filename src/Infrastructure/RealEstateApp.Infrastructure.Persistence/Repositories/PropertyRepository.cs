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
                .Include(p => p.Images)
                .Include(p => p.PropertyImprovements!)
                    .ThenInclude(pi => pi.Improvement)
                .Include(p => p.Favorites)
                .Include(p => p.Offers)
                .Include(p => p.Chats)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
