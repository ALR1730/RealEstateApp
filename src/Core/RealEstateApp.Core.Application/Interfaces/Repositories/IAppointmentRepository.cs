using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository : IGenericRepository<PropertyAppointment>
    {
<<<<<<< HEAD
        Task<List<PropertyAppointment>> GetByAgentIdAsync(string agentId);
        Task<List<PropertyAppointment>> GetByClienteIdAsync(string clienteId);
        Task<List<PropertyAppointment>> GetByPropertyIdAsync(int propertyId);
=======
        Task<List<PropertyAppointment>> GetByPropertyIdAsync(int propertyId);
        Task<List<PropertyAppointment>> GetByClienteIdAsync(string clienteId);
        Task<List<PropertyAppointment>> GetByAgentIdAsync(string agentId);
>>>>>>> feature/filtros-catalogo
    }
}
