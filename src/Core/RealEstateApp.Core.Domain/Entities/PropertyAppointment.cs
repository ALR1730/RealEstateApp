using System;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Domain.Entities
{
    public class PropertyAppointment : AuditableBaseEntity
    {
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public string ClienteId { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;

        public string? Notes { get; set; }
        public string? AgentNotes { get; set; }

        #region Domain Behaviors & Invariants

        public static PropertyAppointment Schedule(
            int propertyId,
            string clienteId,
            string agentId,
            DateTime appointmentDate,
            string? notes = null)
        {
            if (propertyId <= 0)
            {
                throw new DomainException("El identificador de la propiedad debe ser mayor a cero.");
            }

            if (string.IsNullOrWhiteSpace(clienteId))
            {
                throw new DomainException("El identificador del cliente es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(agentId))
            {
                throw new DomainException("El identificador del agente es obligatorio.");
            }

            var referenceNow = appointmentDate.Kind == DateTimeKind.Utc ? DateTime.UtcNow : DateTime.Now;
            if (appointmentDate == DateTime.MinValue || appointmentDate <= referenceNow)
            {
                throw new DomainException("La fecha propuesta para la cita debe ser en el futuro.");
            }

            return new PropertyAppointment
            {
                PropertyId = propertyId,
                ClienteId = clienteId,
                AgentId = agentId,
                AppointmentDate = appointmentDate,
                Status = AppointmentStatus.Pending,
                Notes = notes
            };
        }

        public void Confirm(string agentId, string? agentNotes = null)
        {
            if (AgentId != agentId)
            {
                throw new DomainException("No tiene permisos para modificar esta cita.");
            }

            if (Status != AppointmentStatus.Pending)
            {
                throw new DomainException($"Solo se pueden confirmar citas pendientes. Estado actual: {Status}");
            }

            Status = AppointmentStatus.Confirmed;
            if (!string.IsNullOrEmpty(agentNotes))
            {
                AgentNotes = agentNotes;
            }
        }

        public void Cancel(string userId, string? reason = null)
        {
            if (ClienteId != userId && AgentId != userId)
            {
                throw new DomainException("No tiene permisos para cancelar esta cita.");
            }

            if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            {
                throw new DomainException($"No se puede cancelar una cita en estado: {Status}");
            }

            Status = AppointmentStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
            {
                AgentNotes = reason;
            }
        }

        public void Complete(string agentId, string? agentNotes = null)
        {
            if (AgentId != agentId)
            {
                throw new DomainException("No tiene permisos para modificar esta cita.");
            }

            if (Status != AppointmentStatus.Pending && Status != AppointmentStatus.Confirmed)
            {
                throw new DomainException($"Solo se pueden completar citas pendientes o confirmadas. Estado actual: {Status}");
            }

            Status = AppointmentStatus.Completed;
            if (!string.IsNullOrEmpty(agentNotes))
            {
                AgentNotes = agentNotes;
            }
        }

        #endregion
    }
}
