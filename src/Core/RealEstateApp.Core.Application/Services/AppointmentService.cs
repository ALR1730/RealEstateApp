using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
<<<<<<< HEAD
using Microsoft.AspNetCore.Identity;
=======
>>>>>>> feature/filtros-catalogo
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
<<<<<<< HEAD
        private readonly IUserActivityService _userActivityService;
        private readonly UserManager<IdentityUser> _userManager;
=======
        private readonly IAccountService _accountService;
        private readonly IUserActivityService _userActivityService;
>>>>>>> feature/filtros-catalogo
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IPropertyRepository propertyRepository,
<<<<<<< HEAD
            IUserActivityService userActivityService,
            UserManager<IdentityUser> userManager,
=======
            IAccountService accountService,
            IUserActivityService userActivityService,
>>>>>>> feature/filtros-catalogo
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _propertyRepository = propertyRepository;
<<<<<<< HEAD
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
=======
            _accountService = accountService;
            _userActivityService = userActivityService;
            _mapper = mapper;
        }

        public async Task<AppointmentViewModel?> GetByIdAsync(int id)
>>>>>>> feature/filtros-catalogo
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return null;

<<<<<<< HEAD
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
=======
            var vm = _mapper.Map<AppointmentViewModel>(appointment);
            await PopulateUserNames(vm);
            return vm;
        }

        public async Task<List<AppointmentViewModel>> GetByClienteIdAsync(string clienteId)
        {
            var appointments = await _appointmentRepository.GetByClienteIdAsync(clienteId);
            var vms = _mapper.Map<List<AppointmentViewModel>>(appointments);
            await PopulateUserNamesBatchAsync(vms);
            return vms;
        }

        public async Task<List<AppointmentViewModel>> GetByAgentIdAsync(string agentId)
        {
            var appointments = await _appointmentRepository.GetByAgentIdAsync(agentId);
            var vms = _mapper.Map<List<AppointmentViewModel>>(appointments);
            await PopulateUserNamesBatchAsync(vms);
            return vms;
        }

        public async Task<List<AppointmentViewModel>> GetByPropertyIdAsync(int propertyId)
        {
            var appointments = await _appointmentRepository.GetByPropertyIdAsync(propertyId);
            var vms = _mapper.Map<List<AppointmentViewModel>>(appointments);
            await PopulateUserNamesBatchAsync(vms);
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

        private async Task PopulateUserNamesBatchAsync(List<AppointmentViewModel> vms)
        {
            if (vms == null || !vms.Any()) return;

            var userIds = vms.Select(v => v.ClienteId)
                             .Concat(vms.Select(v => v.AgentId))
                             .Where(id => !string.IsNullOrWhiteSpace(id))
                             .Distinct()!;

            var userDict = await _accountService.GetUsersByIdsAsync(userIds!);

            foreach (var vm in vms)
            {
                if (!string.IsNullOrEmpty(vm.ClienteId) && userDict.TryGetValue(vm.ClienteId, out var clientUser))
                {
                    vm.ClienteName = $"{clientUser.FirstName} {clientUser.LastName}".Trim();
                }
                else if (string.IsNullOrEmpty(vm.ClienteName))
                {
                    vm.ClienteName = "Cliente";
                }

                if (!string.IsNullOrEmpty(vm.AgentId) && userDict.TryGetValue(vm.AgentId, out var agentUser))
                {
                    vm.AgentName = $"{agentUser.FirstName} {agentUser.LastName}".Trim();
                }
                else if (string.IsNullOrEmpty(vm.AgentName))
                {
                    vm.AgentName = "Agente";
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
>>>>>>> feature/filtros-catalogo
    }
}
