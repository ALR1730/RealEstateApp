using System;

namespace RealEstateApp.Core.Application.ViewModels.Offer
{
    /// <summary>
    /// ViewModel de lectura para mostrar ofertas recibidas y contra-ofertas.
    /// </summary>
    public class OfferViewModel
    {
        public int Id { get; set; }
        public decimal MontoOfertado { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteName { get; set; } = string.Empty;
        public DateTime FechaOferta { get; set; }
        public string? PreApprovalLetterUrl { get; set; }
        public string? Notes { get; set; }

        // Campos de Contra-Oferta (Ítem 2.1)
        public decimal? CounterOfferAmount { get; set; }
        public string? CounterOfferMessage { get; set; }
        public DateTime? CounterOfferDate { get; set; }

        // Datos de la propiedad
        public int PropertyId { get; set; }
        public string PropertyCode { get; set; } = string.Empty;
        public decimal PropertyPrice { get; set; }
        public string AgentId { get; set; } = string.Empty;

        public string StatusFormatted => Status switch
        {
            "Pending" => "Pendiente",
            "Accepted" => "Aceptada",
            "Rejected" => "Rechazada",
            "CounterOffered" => "Contra-Ofertada",
            _ => Status
        };
    }
}
