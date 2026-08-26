using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IBuyAbilityEvaluationRepository : IGenericRepository<BuyAbilityEvaluation>
    {
        Task<BuyAbilityEvaluation?> GetLatestByClientIdAsync(string clientId);
    }
}
