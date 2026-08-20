using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.DTOs.Agent
{
    /// <summary>
    /// DTO de agente inmobiliario para exposición en la API REST.
    /// </summary>
    public class AgentDto
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        /// <summary>
        /// Cantidad de propiedades gestionadas por el agente.
        /// </summary>
        public int PropertiesCount { get; set; }

        /// <summary>
        /// Indica si el agente está activo en la plataforma.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indica si el agente tiene su identidad formalmente verificada.
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Resumen de propiedades del agente (para endpoint de detalle).
        /// </summary>
        public List<AgentPropertyDto>? Properties { get; set; }
    }
}
