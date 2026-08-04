using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IUserActivityRepository : IGenericRepository<UserActivity>
    {
        Task<List<UserActivity>> GetByUserIdAsync(string userId, int count);
    }
}
