using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Appointment;

namespace RealEstateApp.Presentation.WebApp.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPropertyService _propertyService;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IPropertyService propertyService,
            UserManager<IdentityUser> userManager)
        {
            _appointmentService = appointmentService;
            _propertyService = propertyService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAppointment(SaveAppointmentViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (vm.AppointmentDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "La fecha y hora de la cita debe ser futura.";
                return RedirectToAction("Details", "Home", new { id = vm.PropertyId });
            }

            try
            {
                await _appointmentService.RequestAppointment(vm, userId);
                TempData["SuccessMessage"] = "¡Tu solicitud de visita ha sido enviada exitosamente al agente!";
                return RedirectToAction(nameof(MyAppointments));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Home", new { id = vm.PropertyId });
            }
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _appointmentService.GetAppointmentsByClientId(userId);
            return View(appointments);
        }

        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AgentCalendar()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _appointmentService.GetAppointmentsByAgentId(userId);
            return View(appointments);
        }

        [Authorize(Roles = "Agent")]
        [HttpGet]
        public async Task<IActionResult> GetAgentCalendarEvents()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Json(new object[] { });

            var appointments = await _appointmentService.GetAppointmentsByAgentId(userId);

            var events = appointments.Select(a => new
            {
                id = a.Id,
                title = $"{a.PropertyCode} - {a.ClienteName}",
                start = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = a.AppointmentDate.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                color = a.Status switch
                {
                    "Pending" => "#f59e0b",
                    "Confirmed" => "#0284c7",
                    "Completed" => "#10b981",
                    "Cancelled" => "#ef4444",
                    _ => "#64748b"
                },
                propertyCode = a.PropertyCode,
                propertyName = a.PropertyName,
                clientName = a.ClienteName,
                status = a.StatusFormatted,
                badgeClass = a.StatusBadgeClass,
                comments = a.Comments ?? "Sin notas adicionales",
                agentNotes = a.AgentNotes ?? ""
            });

            return Json(events);
        }

        [Authorize(Roles = "Agent")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id, string? notes)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            try
            {
                await _appointmentService.ConfirmAppointment(id, userId, notes);
                TempData["SuccessMessage"] = "Cita confirmada exitosamente en tu agenda.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(AgentCalendar));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            try
            {
                await _appointmentService.CancelAppointment(id, userId);
                TempData["SuccessMessage"] = "Cita cancelada correctamente.";
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

        [Authorize(Roles = "Agent")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            try
            {
                await _appointmentService.CompleteAppointment(id, userId);
                TempData["SuccessMessage"] = "Visita marcada como realizada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(AgentCalendar));
        }
    }
}
