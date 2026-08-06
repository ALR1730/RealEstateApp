using System;

namespace RealEstateApp.Core.Application.ViewModels.Appointment
{
    /// <summary>
    /// ViewModel de lectura para la agenda de citas del agente y cliente.
    /// Contiene datos formateados para FullCalendar.js y vistas Razor.
    /// </summary>
    public class PropertyAppointmentViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyCode { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyTypeName { get; set; } = string.Empty;
        public string? PropertyImage { get; set; }

        public string ClienteId { get; set; } = string.Empty;
        public string ClienteName { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }
        public string? Comments { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AgentNotes { get; set; }

        public string StatusFormatted => Status switch
        {
            "Pending" => "Pendiente de Confirmación",
            "Confirmed" => "Confirmada / Agendada",
            "Cancelled" => "Cancelada",
            "Completed" => "Realizada / Finalizada",
            _ => Status
        };

        public string StatusBadgeClass => Status switch
        {
            "Pending" => "bg-warning text-dark",
            "Confirmed" => "bg-primary text-white",
            "Cancelled" => "bg-danger text-white",
            "Completed" => "bg-success text-white",
            _ => "bg-secondary text-white"
        };
    }
}
