using System;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Entidad para el sistema de Valuación Automatizada (AVM - Automated Valuation Model).
    /// Almacena estimaciones de precio por m² basadas en comparables de la misma provincia/sector.
    /// </summary>
    public class PropertyValuation : AuditableBaseEntity
    {
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        /// <summary>
        /// Precio estimado por metro cuadrado basado en comparables.
        /// </summary>
        public decimal EstimatedPricePerSqm { get; set; }

        /// <summary>
        /// Precio estimado total de la propiedad.
        /// </summary>
        public decimal EstimatedTotalPrice { get; set; }

        /// <summary>
        /// Cantidad de propiedades comparables utilizadas para el cálculo.
        /// </summary>
        public int ComparableCount { get; set; }

        /// <summary>
        /// Rango de precios por m² de los comparables (mínimo).
        /// </summary>
        public decimal MinPricePerSqm { get; set; }

        /// <summary>
        /// Rango de precios por m² de los comparables (máximo).
        /// </summary>
        public decimal MaxPricePerSqm { get; set; }

        /// <summary>
        /// Desviación estándar del precio por m² de los comparables.
        /// </summary>
        public decimal StandardDeviation { get; set; }

        /// <summary>
        /// Radio de búsqueda en kilómetros para encontrar comparables.
        /// </summary>
        public decimal SearchRadiusKm { get; set; } = 5.0m;

        /// <summary>
        /// Moneda de la valuación (DOP/USD).
        /// </summary>
        public string Currency { get; set; } = "DOP";

        /// <summary>
        /// Nivel de confianza de la estimación (0-100).
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// Fecha de la última actualización de la tasación.
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Notas adicionales sobre la valuación.
        /// </summary>
        public string? Notes { get; set; }
    }
}
