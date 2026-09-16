<<<<<<< HEAD
using System;
=======
>>>>>>> feature/filtros-catalogo
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Appointment;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
<<<<<<< HEAD
    /// <summary>
    /// Servicio de aplicación para la gestión de la agenda de citas e integración con calendario (Ítem 2.2).
    /// </summary>
    public interface IAppointmentService
    {
        Task<SaveAppointmentViewModel> RequestAppointment(SaveAppointmentViewModel vm, string clientUserId);
        Task<List<PropertyAppointmentViewModel>> GetAppointmentsByAgentId(string agentId);
        Task<List<PropertyAppointmentViewModel>> GetAppointmentsByClientId(string clientId);
        Task<PropertyAppointmentViewModel?> GetById(int id);

        Task ConfirmAppointment(int id, string agentUserId, string? notes);
        Task CancelAppointment(int id, string userId);
        Task CompleteAppointment(int id, string agentUserId);
=======
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
>>>>>>> feature/filtros-catalogo
    }
}
