using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<PropertyAppointment>
    {
        Task<List<PropertyAppointment>> GetByPropertyIdAsync(int propertyId);
        Task<List<PropertyAppointment>> GetByClienteIdAsync(string clienteId);
        Task<List<PropertyAppointment>> GetByAgentIdAsync(string agentId);
    }
}
