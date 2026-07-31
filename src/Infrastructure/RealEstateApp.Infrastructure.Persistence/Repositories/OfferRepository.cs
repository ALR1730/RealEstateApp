using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
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

        public async Task AcceptOfferTransactionAsync(int offerId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var offer = await _dbContext.Set<Offer>().FirstOrDefaultAsync(o => o.Id == offerId);
                if (offer == null)
                {
                    throw new NotFoundException("La oferta no existe");
                }

                if (offer.Status != RealEstateApp.Core.Domain.Enums.OfferStatus.Pending)
                {
                    throw new ValidationException("Solo se pueden aceptar ofertas pendientes");
                }

                // 1. Aceptar la oferta elegida
                offer.Status = RealEstateApp.Core.Domain.Enums.OfferStatus.Accepted;

                // 2. Marcar la propiedad como "Vendida"
                var property = await _dbContext.Set<Property>().FirstOrDefaultAsync(p => p.Id == offer.PropertyId);
                if (property != null)
                {
                    property.Status = PropertyStatus.Sold;
                }

                // 3. Rechazar en cascada todas las demás ofertas pendientes de esa propiedad
                var otherOffers = await _dbContext.Set<Offer>()
                    .Where(o => o.PropertyId == offer.PropertyId && o.Id != offerId && o.Status == RealEstateApp.Core.Domain.Enums.OfferStatus.Pending)
                    .ToListAsync();

                foreach (var otherOffer in otherOffers)
                {
                    otherOffer.Status = RealEstateApp.Core.Domain.Enums.OfferStatus.Rejected;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
