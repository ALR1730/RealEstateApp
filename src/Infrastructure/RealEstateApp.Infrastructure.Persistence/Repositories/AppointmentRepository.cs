using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Contexts;

namespace RealEstateApp.Infrastructure.Persistence.Repositories
{
    public class AppointmentRepository : GenericRepository<PropertyAppointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<PropertyAppointment>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<PropertyAppointment>()
                .Include(pa => pa.Property)
                    .ThenInclude(p => p!.Images)
                .Where(pa => pa.PropertyId == propertyId)
                .OrderByDescending(pa => pa.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<PropertyAppointment>> GetByClienteIdAsync(string clienteId)
        {
            return await _dbContext.Set<PropertyAppointment>()
                .Include(pa => pa.Property)
                    .ThenInclude(p => p!.Images)
                .Where(pa => pa.ClienteId == clienteId)
                .OrderByDescending(pa => pa.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<PropertyAppointment>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<PropertyAppointment>()
                .Include(pa => pa.Property)
                    .ThenInclude(p => p!.Images)
                .Where(pa => pa.AgentId == agentId)
                .OrderByDescending(pa => pa.AppointmentDate)
                .ToListAsync();
        }
    }
}
