using System;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Entidad para el sistema CRM Kanban de gestión de leads inmobiliarios.
    /// Representa un lead en el pipeline de ventas con su estado de avance.
    /// </summary>
    public class LeadPipeline : AuditableBaseEntity
    {
        /// <summary>
        /// Nombre completo del lead (cliente potencial).
        /// </summary>
        public string LeadName { get; set; } = string.Empty;

        /// <summary>
        /// Email del lead.
        /// </summary>
        public string? LeadEmail { get; set; }

        /// <summary>
        /// Teléfono del lead.
        /// </summary>
        public string? LeadPhone { get; set; }

        /// <summary>
        /// Notas generales sobre el lead.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// ID del agente asignado al lead.
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        /// <summary>
        /// Estado actual del lead en el pipeline (Nuevo Lead, Contactado, Visita, Oferta, Cierre).
        /// </summary>
        public string Stage { get; set; } = PipelineStage.NewLead;

        /// <summary>
        /// Prioridad del lead (Baja, Normal, Alta, Urgente).
        /// </summary>
        public string Priority { get; set; } = "Normal";

        /// <summary>
        /// Fuente de origen del lead (Portal, Referido, Redes Sociales, etc.).
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Presupuesto estimado del lead.
        /// </summary>
        public decimal? EstimatedBudget { get; set; }

        /// <summary>
        /// Moneda del presupuesto.
        /// </summary>
        public string? BudgetCurrency { get; set; }

        /// <summary>
        /// ID de la propiedad de interés (si aplica).
        /// </summary>
        public int? PropertyId { get; set; }
        public Property? Property { get; set; }

        /// <summary>
        /// Fecha de la última interacción con el lead.
        /// </summary>
        public DateTime? LastContactDate { get; set; }

        /// <summary>
        /// Fecha programada para la próxima interacción.
        /// </summary>
        public DateTime? NextFollowUpDate { get; set; }

        /// <summary>
        /// Posición de orden dentro de la columna del kanban.
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// Constantes para las etapas del pipeline Kanban.
    /// </summary>
    public static class PipelineStage
    {
        public const string NewLead = "Nuevo Lead";
        public const string Contacted = "Contactado";
        public const string Visit = "Visita";
        public const string Offer = "Oferta";
        public const string Closing = "Cierre";
        public const string Won = "Ganado";
        public const string Lost = "Perdido";
    }
}
