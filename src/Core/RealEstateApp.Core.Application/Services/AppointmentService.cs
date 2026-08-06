using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Appointment;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IUserActivityService _userActivityService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IPropertyRepository propertyRepository,
            IUserActivityService userActivityService,
            UserManager<IdentityUser> userManager,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _propertyRepository = propertyRepository;
            _userActivityService = userActivityService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<SaveAppointmentViewModel> RequestAppointment(SaveAppointmentViewModel vm, string clientUserId)
        {
            var property = await _propertyRepository.GetByIdAsync(vm.PropertyId);
            if (property == null)
                throw new NotFoundException("La propiedad solicitada no existe");

            var appointment = new PropertyAppointment
            {
                PropertyId = vm.PropertyId,
                ClienteId = clientUserId,
                AgentId = property.AgentId,
                AppointmentDate = vm.AppointmentDate,
                Comments = vm.Comments,
                Status = AppointmentStatus.Pending
            };

            await _appointmentRepository.AddAsync(appointment);

            await _userActivityService.LogActivityAsync(
                clientUserId,
                "Cita de Visita Solicitada",
                $"Solicitó una visita para la propiedad (Cód: {property.Code}) el {vm.AppointmentDate:dd/MM/yyyy hh:mm tt}",
                "bi-calendar-event text-primary",
                $"/Home/Details/{property.Id}"
            );

            return vm;
        }

        public async Task<List<PropertyAppointmentViewModel>> GetAppointmentsByAgentId(string agentId)
        {
            var appointments = await _appointmentRepository.GetByAgentIdAsync(agentId);
            var result = _mapper.Map<List<PropertyAppointmentViewModel>>(appointments);

            foreach (var item in result)
            {
                var clientUser = await _userManager.FindByIdAsync(item.ClienteId);
                item.ClienteName = clientUser?.UserName ?? "Cliente";
            }

            return result;
        }

        public async Task<List<PropertyAppointmentViewModel>> GetAppointmentsByClientId(string clientId)
        {
            var appointments = await _appointmentRepository.GetByClienteIdAsync(clientId);
            var result = _mapper.Map<List<PropertyAppointmentViewModel>>(appointments);

            foreach (var item in result)
            {
                var agentUser = await _userManager.FindByIdAsync(item.AgentId);
                item.AgentName = agentUser?.UserName ?? "Agente";
            }

            return result;
        }

        public async Task<PropertyAppointmentViewModel?> GetById(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return null;

            return _mapper.Map<PropertyAppointmentViewModel>(appointment);
        }

        public async Task ConfirmAppointment(int id, string agentUserId, string? notes)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new NotFoundException("La cita no fue encontrada");

            if (appointment.AgentId != agentUserId)
                throw new ValidationException("No tiene permisos para confirmar esta cita");

            appointment.Status = AppointmentStatus.Confirmed;
            appointment.AgentNotes = notes;

            await _appointmentRepository.UpdateAsync(appointment);

            await _userActivityService.LogActivityAsync(
                agentUserId,
                "Cita de Visita Confirmada",
                $"Confirmó la visita para el {appointment.AppointmentDate:dd/MM/yyyy hh:mm tt}",
                "bi-calendar-check text-success",
                "/Appointments/AgentCalendar"
            );
        }

        public async Task CancelAppointment(int id, string userId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new NotFoundException("La cita no existe");

            if (appointment.AgentId != userId && appointment.ClienteId != userId)
                throw new ValidationException("No tiene autorización para modificar esta cita");

            appointment.Status = AppointmentStatus.Cancelled;
            await _appointmentRepository.UpdateAsync(appointment);

            await _userActivityService.LogActivityAsync(
                userId,
                "Cita de Visita Cancelada",
                $"Canceló la cita agendada para el {appointment.AppointmentDate:dd/MM/yyyy hh:mm tt}",
                "bi-calendar-x text-danger",
                "/Appointments/MyAppointments"
            );
        }

        public async Task CompleteAppointment(int id, string agentUserId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
                throw new NotFoundException("La cita no existe");

            if (appointment.AgentId != agentUserId)
                throw new ValidationException("No tiene permisos para modificar esta cita");

            appointment.Status = AppointmentStatus.Completed;
            await _appointmentRepository.UpdateAsync(appointment);

            await _userActivityService.LogActivityAsync(
                agentUserId,
                "Visita Realizada",
                $"Marcó como completada la visita presencial a la propiedad",
                "bi-calendar-check-fill text-success",
                "/Appointments/AgentCalendar"
            );
        }
    }
}
