using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Appointment
{
<<<<<<< HEAD
    /// <summary>
    /// ViewModel para la solicitud de agendamiento de cita/visita presencial por el cliente.
    /// </summary>
=======
>>>>>>> feature/filtros-catalogo
    public class SaveAppointmentViewModel
    {
        public int Id { get; set; }

<<<<<<< HEAD
        [Required(ErrorMessage = "La propiedad es requerida")]
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de la cita es requerida")]
        [Display(Name = "Fecha y Hora de Visita")]
        public DateTime AppointmentDate { get; set; } = DateTime.Now.AddDays(1);

        [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder los 500 caracteres")]
        [Display(Name = "Comentarios o Preferencias del Cliente")]
        public string? Comments { get; set; }

        // Campos auxiliares para la vista
        public string? PropertyCode { get; set; }
        public string? PropertyName { get; set; }
=======
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
>>>>>>> feature/filtros-catalogo
    }
}
