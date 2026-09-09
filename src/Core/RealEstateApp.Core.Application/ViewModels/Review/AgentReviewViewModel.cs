using System;

namespace RealEstateApp.Core.Application.ViewModels.Review
{
    public class AgentReviewViewModel
    {
        public int Id { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public int PropertyId { get; set; }
        public string? PropertyCode { get; set; }
        public string? PropertyName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime Created { get; set; }

        // Datos del cliente (identidad visible para admin y agente)
        public string? ClienteName { get; set; }
        public string? ClienteEmail { get; set; }
    }
}
