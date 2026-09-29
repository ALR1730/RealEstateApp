using System;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.DTOs.Offer
{
    /// <summary>
    /// Contrato de entrada (Comando) para registrar una oferta en el caso de uso.
    /// Reemplaza el uso de SaveOfferViewModel para nuevas implementaciones en Core.
    /// </summary>
    public class CreateOfferRequest
    {
        public int PropertyId { get; set; }
        public decimal MontoOfertado { get; set; }
        public string? PreApprovalLetterUrl { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Contrato de salida (Respuesta) con datos de una oferta para los clientes del caso de uso.
    /// </summary>
    public class OfferResponse
    {
        public int Id { get; set; }
        public decimal MontoOfertado { get; set; }
        public OfferStatus Status { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public DateTime FechaOferta { get; set; }
        public string? PreApprovalLetterUrl { get; set; }
        public string? Notes { get; set; }
        public decimal? CounterOfferAmount { get; set; }
        public string? CounterOfferMessage { get; set; }
        public DateTime? CounterOfferDate { get; set; }
        public int PropertyId { get; set; }
        public string? PropertyName { get; set; }
    }
}
