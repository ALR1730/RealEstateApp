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
    public class ReviewRepository : GenericRepository<AgentReview>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<bool> HasCompletedTransactionAsync(string clienteId, string agentId, int propertyId)
        {
            return await _dbContext.Set<Offer>()
                .AnyAsync(o => o.ClienteId == clienteId &&
                               o.PropertyId == propertyId &&
                               o.Status == OfferStatus.Accepted &&
                               o.Property != null &&
                               o.Property.AgentId == agentId);
        }

        public async Task<bool> ExistsAsync(string clienteId, string agentId, int propertyId)
        {
            return await _dbContext.Set<AgentReview>()
                .AnyAsync(r => r.ClienteId == clienteId &&
                               r.AgentId == agentId &&
                               r.PropertyId == propertyId);
        }

        public async Task<List<AgentReview>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<AgentReview>()
                .Include(r => r.Property)
                .Where(r => r.AgentId == agentId)
                .OrderByDescending(r => r.Created)
                .ToListAsync();
        }

        public async Task<List<AgentReview>> GetAllWithDetailsAsync()
        {
            return await _dbContext.Set<AgentReview>()
                .Include(r => r.Property)
                .OrderByDescending(r => r.Created)
                .ToListAsync();
        }
    }
}
