using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class OfferRepository : GenericRepository<Offer>, IOfferRepository
    {
        public OfferRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<Offer>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<Offer>()
                .Include(o => o.Property)
                .Where(o => o.PropertyId == propertyId)
                .OrderByDescending(o => o.FechaOferta)
                .ToListAsync();
        }

        public async Task<List<Offer>> GetByClienteIdAsync(string clienteId)
        {
            return await _dbContext.Set<Offer>()
                .Include(o => o.Property)
                .Where(o => o.ClienteId == clienteId)
                .OrderByDescending(o => o.FechaOferta)
                .ToListAsync();
        }

        public async Task<List<Offer>> GetByPropertyIdsAsync(IEnumerable<int> propertyIds)
        {
            return await _dbContext.Set<Offer>()
                .Include(o => o.Property)
                .Where(o => propertyIds.Contains(o.PropertyId))
                .OrderByDescending(o => o.FechaOferta)
                .ToListAsync();
        }

        public override async Task<List<Offer>> GetAllAsync()
        {
            return await _dbContext.Set<Offer>()
                .Include(o => o.Property)
                .OrderByDescending(o => o.FechaOferta)
                .ToListAsync();
        }

        public async Task<List<Offer>> GetActiveCompetingOffersAsync(int propertyId, int excludeOfferId)
        {
            return await _dbContext.Set<Offer>()
                .Where(o => o.PropertyId == propertyId
                            && o.Id != excludeOfferId
                            && (o.Status == OfferStatus.Pending || o.Status == OfferStatus.CounterOffered))
                .ToListAsync();
        }
    }
}
