using System;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Domain.Entities
{
    public class PropertyAppointment : AuditableBaseEntity
    {
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public string ClienteId { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string? Notes { get; set; }
        public string? AgentNotes { get; set; }
    }
}
