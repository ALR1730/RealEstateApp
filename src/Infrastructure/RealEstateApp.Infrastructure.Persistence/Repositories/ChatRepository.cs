using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class ChatRepository : GenericRepository<Chat>, IChatRepository
    {
        public ChatRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public async Task<List<Chat>> GetChatThreadAsync(string clienteId, string agenteId, int propertyId)
        {
            return await _dbContext.Set<Chat>()
                .Include(c => c.Property)
                .Where(c => c.ClienteId == clienteId && c.AgenteId == agenteId && c.PropertyId == propertyId)
                .OrderBy(c => c.SentAt)
                .ToListAsync();
        }

        public async Task<List<Chat>> GetByUserIdAsync(string userId)
        {
            return await _dbContext.Set<Chat>()
                .Include(c => c.Property)
                .Where(c => c.ClienteId == userId || c.AgenteId == userId)
                .OrderByDescending(c => c.SentAt)
                .ToListAsync();
        }
    }
}
