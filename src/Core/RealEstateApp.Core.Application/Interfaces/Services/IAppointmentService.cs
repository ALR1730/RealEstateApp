using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Appointment;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
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
    }
}
