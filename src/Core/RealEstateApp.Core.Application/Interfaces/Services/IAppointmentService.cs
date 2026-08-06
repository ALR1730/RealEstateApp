using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Appointment;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentViewModel?> GetByIdAsync(int id);
        Task<List<AppointmentViewModel>> GetByClienteIdAsync(string clienteId);
        Task<List<AppointmentViewModel>> GetByAgentIdAsync(string agentId);
        Task<List<AppointmentViewModel>> GetByPropertyIdAsync(int propertyId);
        Task<AppointmentViewModel> RequestAppointmentAsync(SaveAppointmentViewModel vm, string clienteId);
        Task ConfirmAppointmentAsync(int appointmentId, string agentId, string? agentNotes = null);
        Task CancelAppointmentAsync(int appointmentId, string userId, string? reason = null);
        Task CompleteAppointmentAsync(int appointmentId, string agentId, string? agentNotes = null);
    }
}
