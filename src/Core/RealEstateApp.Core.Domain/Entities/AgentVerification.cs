using System;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Solicitud y registro de verificación de identidad de un Agente Inmobiliario
    /// mediante Cédula de Identidad y Electoral dominicana o documento de identidad oficial.
    /// </summary>
    public class AgentVerification : AuditableBaseEntity
    {
        public string AgentId { get; set; } = string.Empty;
        
        public string Cedula { get; set; } = string.Empty;
        public string CedulaFrontImageUrl { get; set; } = string.Empty;
        public string CedulaBackImageUrl { get; set; } = string.Empty;

        public string Status { get; set; } = VerificationStatus.Pending;
        public string? RejectionReason { get; set; }

        public string? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
