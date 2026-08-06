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

        public async Task<List<PropertyAppointment>> GetByAgentIdAsync(string agentId)
        {
            return await _dbContext.Set<PropertyAppointment>()
                .Include(a => a.Property)
                    .ThenInclude(p => p!.Images)
                .Include(a => a.Property)
                    .ThenInclude(p => p!.PropertyType)
                .Where(a => a.AgentId == agentId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<PropertyAppointment>> GetByClienteIdAsync(string clienteId)
        {
            return await _dbContext.Set<PropertyAppointment>()
                .Include(a => a.Property)
                    .ThenInclude(p => p!.Images)
                .Include(a => a.Property)
                    .ThenInclude(p => p!.PropertyType)
                .Where(a => a.ClienteId == clienteId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<List<PropertyAppointment>> GetByPropertyIdAsync(int propertyId)
        {
            return await _dbContext.Set<PropertyAppointment>()
                .Include(a => a.Property)
                .Where(a => a.PropertyId == propertyId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }
    }
}
