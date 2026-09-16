using System;
using System.Text.Json.Serialization;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.ViewModels.Appointment
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string PropertyCode { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string PropertyMainImage { get; set; } = string.Empty;

        public string ClienteId { get; set; } = string.Empty;
        public string ClienteName { get; set; } = string.Empty;

        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppointmentStatus Status { get; set; }
        public string StatusFormatted { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public string? AgentNotes { get; set; }
        public DateTime Created { get; set; }
    }
}
