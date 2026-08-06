using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Appointment;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentsController(
            IAppointmentService appointmentService,
            UserManager<IdentityUser> userManager)
        {
            _appointmentService = appointmentService;
            _userManager = userManager;
        }

        /// <summary>
        /// Solicitud de Cita enviada por un Cliente desde la ficha del inmueble (Details.cshtml).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAppointment(SaveAppointmentViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var appDate = vm.GetParsedAppointmentDate();
            if (vm.PropertyId <= 0 || appDate == DateTime.MinValue || appDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Por favor ingrese una fecha y hora válidas (en el futuro) para la cita.";
                return RedirectToAction("Details", "Home", new { id = vm.PropertyId });
            }

            try
            {
                var appointment = await _appointmentService.RequestAppointmentAsync(vm, userId);
                TempData["SuccessMessage"] = $"¡Solicitud de cita para el {appointment.AppointmentDate:dd/MM/yyyy hh:mm tt} enviada con éxito al agente!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Details", "Home", new { id = vm.PropertyId });
        }

        /// <summary>
        /// Vista de Citas del Cliente (Mis Citas).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _appointmentService.GetByClienteIdAsync(userId);
            return View(appointments);
        }

        /// <summary>
        /// Calendario y Gestión de Citas para el Agente Inmobiliario.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AgentCalendar()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _appointmentService.GetByAgentIdAsync(userId);
            return View(appointments);
        }

        /// <summary>
        /// Endpoint JSON para renderizado de eventos en FullCalendar.js en la agenda del agente.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var appointments = await _appointmentService.GetByAgentIdAsync(userId);

            var events = appointments.Select(a =>
            {
                string color = "#3b82f6"; // Blue for pending
                string border = "#2563eb";
                if (a.Status == AppointmentStatus.Confirmed)
                {
                    color = "#10b981"; // Emerald green for confirmed
                    border = "#059669";
                }
                else if (a.Status == AppointmentStatus.Cancelled)
                {
                    color = "#f43f5e"; // Rose red for cancelled
                    border = "#e11d48";
                }
                else if (a.Status == AppointmentStatus.Completed)
                {
                    color = "#64748b"; // Slate gray for completed
                    border = "#475569";
                }

                return new
                {
                    id = a.Id,
                    title = $"{a.PropertyName} - {a.ClienteName}",
                    start = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = a.AppointmentDate.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    backgroundColor = color,
                    borderColor = border,
                    textColor = "#ffffff",
                    extendedProps = new
                    {
                        propertyCode = a.PropertyCode,
                        propertyName = a.PropertyName,
                        clientName = a.ClienteName,
                        status = a.Status.ToString(),
                        statusFormatted = a.StatusFormatted,
                        notes = a.Notes,
                        agentNotes = a.AgentNotes,
                        propertyId = a.PropertyId
                    }
                };
            });

            return Json(events);
        }

        /// <summary>
        /// Confirmar una cita por parte del agente.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Agent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, string? agentNotes)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _appointmentService.ConfirmAppointmentAsync(id, userId, agentNotes);
                TempData["SuccessMessage"] = "La cita fue confirmada exitosamente. Se ha notificado al cliente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(AgentCalendar));
        }

        /// <summary>
        /// Cancelar una cita (Cliente o Agente).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _appointmentService.CancelAppointmentAsync(id, userId, reason);
                TempData["SuccessMessage"] = "La cita fue cancelada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            if (User.IsInRole("Agent"))
            {
                return RedirectToAction(nameof(AgentCalendar));
            }

            return RedirectToAction(nameof(MyAppointments));
        }

        /// <summary>
        /// Marcar una cita como completada por parte del agente.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Agent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, string? agentNotes)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _appointmentService.CompleteAppointmentAsync(id, userId, agentNotes);
                TempData["SuccessMessage"] = "Cita marcada como completada con éxito.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(AgentCalendar));
        }
    }
}
