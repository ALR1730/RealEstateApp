using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Appointment
{
    /// <summary>
    /// ViewModel para la solicitud de agendamiento de cita/visita presencial por el cliente.
    /// </summary>
    public class SaveAppointmentViewModel
    {
        public int Id { get; set; }

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
    }
}
