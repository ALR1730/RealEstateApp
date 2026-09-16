using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface ISubscriptionPlanRepository : IGenericRepository<SubscriptionPlan>
    {
        Task<List<SubscriptionPlan>> GetActivePlansAsync();
        Task<SubscriptionPlan?> GetDefaultFreePlanAsync();
    }
}
