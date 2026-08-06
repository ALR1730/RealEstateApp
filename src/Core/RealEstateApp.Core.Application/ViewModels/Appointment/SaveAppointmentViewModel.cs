using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Appointment
{
    public class SaveAppointmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La propiedad es requerida.")]
        public int PropertyId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public string? rawAppointmentDate { get; set; }

        [MaxLength(1000, ErrorMessage = "Los comentarios no pueden exceder de 1000 caracteres.")]
        public string? Comments { get; set; }

        public string? AgentNotes { get; set; }

        public DateTime GetParsedAppointmentDate()
        {
            if (AppointmentDate.HasValue && AppointmentDate.Value > DateTime.MinValue)
            {
                return AppointmentDate.Value;
            }

            if (!string.IsNullOrEmpty(rawAppointmentDate) && DateTime.TryParse(rawAppointmentDate, out var parsed))
            {
                return parsed;
            }

            return DateTime.MinValue;
        }
    }
}
