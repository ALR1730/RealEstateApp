using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class BuyAbilityEvaluationRepository : GenericRepository<BuyAbilityEvaluation>, IBuyAbilityEvaluationRepository
    {
        public BuyAbilityEvaluationRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<BuyAbilityEvaluation?> GetLatestByClientIdAsync(string clientId)
        {
            return await _dbContext.BuyAbilityEvaluations
                .Where(ba => ba.ClientId == clientId && ba.IsActive)
                .OrderByDescending(ba => ba.EvaluationDate)
                .FirstOrDefaultAsync();
        }
    }
}
