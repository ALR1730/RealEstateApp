using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<Favorite>> GetByClienteIdAsync(string clienteId)
        {
            return await _dbContext.Set<Favorite>()
                .Include(f => f.Property)
                    .ThenInclude(p => p!.PropertyType)
                .Include(f => f.Property)
                    .ThenInclude(p => p!.Images)
                .Where(f => f.ClienteId == clienteId)
                // Depuración automática: excluir propiedades vendidas
                .Where(f => f.Property != null && f.Property.Status != "Vendida")
                .OrderByDescending(f => f.Created)
                .ToListAsync();
        }

        public async Task<Favorite?> GetByClienteAndPropertyAsync(string clienteId, int propertyId)
        {
            return await _dbContext.Set<Favorite>()
                .FirstOrDefaultAsync(f => f.ClienteId == clienteId && f.PropertyId == propertyId);
        }
    }
}
