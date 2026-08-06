using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IAccountService _accountService;
        private readonly IUserActivityService _userActivityService;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IPropertyRepository propertyRepository,
            IAccountService accountService,
            IUserActivityService userActivityService,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _propertyRepository = propertyRepository;
            _accountService = accountService;
            _userActivityService = userActivityService;
            _mapper = mapper;
        }

        public async Task<AppointmentViewModel?> GetByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return null;

            var vm = _mapper.Map<AppointmentViewModel>(appointment);
            await PopulateUserNames(vm);
            return vm;
        }

        public async Task<List<AppointmentViewModel>> GetByClienteIdAsync(string clienteId)
        {
            var appointments = await _appointmentRepository.GetByClienteIdAsync(clienteId);
            var vms = _mapper.Map<List<AppointmentViewModel>>(appointments);
            foreach (var vm in vms)
            {
                await PopulateUserNames(vm);
            }
            return vms;
        }

        public async Task<List<AppointmentViewModel>> GetByAgentIdAsync(string agentId)
        {
            var appointments = await _appointmentRepository.GetByAgentIdAsync(agentId);
            var vms = _mapper.Map<List<AppointmentViewModel>>(appointments);
            foreach (var vm in vms)
            {
                await PopulateUserNames(vm);
            }
            return vms;
        }

        public async Task<List<AppointmentViewModel>> GetByPropertyIdAsync(int propertyId)
        {
            var appointments = await _appointmentRepository.GetByPropertyIdAsync(propertyId);
            var vms = _mapper.Map<List<AppointmentViewModel>>(appointments);
            foreach (var vm in vms)
            {
                await PopulateUserNames(vm);
            }
            return vms;
        }

        public async Task<AppointmentViewModel> RequestAppointmentAsync(SaveAppointmentViewModel vm, string clienteId)
        {
            var property = await _propertyRepository.GetByIdAsync(vm.PropertyId);
            if (property == null)
            {
                throw new DomainException("La propiedad especificada no existe.");
            }

            DateTime appDate = vm.GetParsedAppointmentDate();

            if (appDate == DateTime.MinValue || appDate <= DateTime.Now)
            {
                throw new DomainException("La fecha propuesta para la cita debe ser en el futuro.");
            }

            var appointment = new PropertyAppointment
            {
                PropertyId = vm.PropertyId,
                ClienteId = clienteId,
                AgentId = property.AgentId,
                AppointmentDate = appDate,
                Status = AppointmentStatus.Pending,
                Notes = vm.Comments
            };

            var created = await _appointmentRepository.AddAsync(appointment);

            // Log activity
            await _userActivityService.LogActivityAsync(
                clienteId,
                "Solicitud de Cita",
                $"Solicitó una visita a la propiedad '{property.Name}' (Cód: {property.Code}) para el {appDate:dd/MM/yyyy hh:mm tt}",
                "bi-calendar-plus",
                $"/Home/Details/{property.Id}"
            );

            var resultVm = _mapper.Map<AppointmentViewModel>(created);
            resultVm.PropertyName = property.Name;
            resultVm.PropertyCode = property.Code;
            await PopulateUserNames(resultVm);
            return resultVm;
        }

        public async Task ConfirmAppointmentAsync(int appointmentId, string agentId, string? agentNotes = null)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                throw new DomainException("La cita no existe.");
            }

            if (appointment.AgentId != agentId)
            {
                throw new DomainException("No tiene permisos para modificar esta cita.");
            }

            appointment.Status = AppointmentStatus.Confirmed;
            if (!string.IsNullOrEmpty(agentNotes))
            {
                appointment.AgentNotes = agentNotes;
            }

            await _appointmentRepository.UpdateAsync(appointment);

            // Log activity for client
            var property = await _propertyRepository.GetByIdAsync(appointment.PropertyId);
            await _userActivityService.LogActivityAsync(
                appointment.ClienteId,
                "Cita Confirmada",
                $"El agente confirmó tu cita de visita a la propiedad '{property?.Name ?? "Inmueble"}' para el {appointment.AppointmentDate:dd/MM/yyyy hh:mm tt}",
                "bi-calendar-check",
                $"/Appointments/MyAppointments"
            );
        }

        public async Task CancelAppointmentAsync(int appointmentId, string userId, string? reason = null)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                throw new DomainException("La cita no existe.");
            }

            if (appointment.ClienteId != userId && appointment.AgentId != userId)
            {
                throw new DomainException("No tiene permisos para cancelar esta cita.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
            {
                appointment.AgentNotes = reason;
            }

            await _appointmentRepository.UpdateAsync(appointment);

            var property = await _propertyRepository.GetByIdAsync(appointment.PropertyId);
            await _userActivityService.LogActivityAsync(
                userId,
                "Cita Cancelada",
                $"Cancelaste la cita de visita a la propiedad '{property?.Name ?? "Inmueble"}'",
                "bi-calendar-x",
                $"/Home/Details/{appointment.PropertyId}"
            );
        }

        public async Task CompleteAppointmentAsync(int appointmentId, string agentId, string? agentNotes = null)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                throw new DomainException("La cita no existe.");
            }

            if (appointment.AgentId != agentId)
            {
                throw new DomainException("No tiene permisos para modificar esta cita.");
            }

            appointment.Status = AppointmentStatus.Completed;
            if (!string.IsNullOrEmpty(agentNotes))
            {
                appointment.AgentNotes = agentNotes;
            }

            await _appointmentRepository.UpdateAsync(appointment);

            var property = await _propertyRepository.GetByIdAsync(appointment.PropertyId);
            await _userActivityService.LogActivityAsync(
                appointment.ClienteId,
                "Visita Completada",
                $"Se realizó con éxito la visita a la propiedad '{property?.Name ?? "Inmueble"}'",
                "bi-calendar-event-fill",
                $"/Home/Details/{appointment.PropertyId}"
            );
        }

        private async Task PopulateUserNames(AppointmentViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.ClienteId))
            {
                var clientUser = await _accountService.GetUserByIdAsync(vm.ClienteId);
                vm.ClienteName = clientUser != null ? $"{clientUser.FirstName} {clientUser.LastName}".Trim() : "Cliente";
            }

            if (!string.IsNullOrEmpty(vm.AgentId))
            {
                var agentUser = await _accountService.GetUserByIdAsync(vm.AgentId);
                vm.AgentName = agentUser != null ? $"{agentUser.FirstName} {agentUser.LastName}".Trim() : "Agente";
            }

            switch (vm.Status)
            {
                case AppointmentStatus.Pending:
                    vm.StatusFormatted = "Pendiente de Confirmación";
                    break;
                case AppointmentStatus.Confirmed:
                    vm.StatusFormatted = "Confirmada";
                    break;
                case AppointmentStatus.Cancelled:
                    vm.StatusFormatted = "Cancelada";
                    break;
                case AppointmentStatus.Completed:
                    vm.StatusFormatted = "Completada";
                    break;
            }
        }
    }
}
