using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class FavoriteRepository : GenericRepository<Favorite>, IFavoriteRepository
    {
        public FavoriteRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<Favorite>> GetByClienteIdAsync(string clienteId)
        {
            var acceptedPropertyIds = await _dbContext.Set<Offer>()
                .Where(o => o.ClienteId == clienteId && (o.Status == OfferStatus.Accepted || o.Status.ToString() == "Aceptada"))
                .Select(o => o.PropertyId)
                .ToListAsync();

            return await _dbContext.Set<Favorite>()
                .Include(f => f.Property)
                    .ThenInclude(p => p!.PropertyType)
                .Include(f => f.Property)
                    .ThenInclude(p => p!.Images)
                .Where(f => f.ClienteId == clienteId)
                // Excluir propiedades vendidas EXCEPTO si el cliente actual tiene una oferta aceptada en esa propiedad
                .Where(f => f.Property != null && (f.Property.Status != PropertyStatus.Sold || acceptedPropertyIds.Contains(f.PropertyId)))
                .OrderByDescending(f => f.Created)
                .ToListAsync();
        }

        public async Task<Favorite?> GetByClienteAndPropertyAsync(string clienteId, int propertyId)
        {
            return await _dbContext.Set<Favorite>()
                .FirstOrDefaultAsync(f => f.ClienteId == clienteId && f.PropertyId == propertyId);
        }

        public async Task<List<Favorite>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<Favorite>()
                .Where(f => f.PropertyId == propertyId)
                .ToListAsync();
        }
    }
}
