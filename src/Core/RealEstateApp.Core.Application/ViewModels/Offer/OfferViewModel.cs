using System;

namespace RealEstateApp.Core.Application.ViewModels.Offer
{
    /// <summary>
    /// ViewModel de lectura para mostrar ofertas recibidas.
    /// </summary>
    public class OfferViewModel
    {
        public int Id { get; set; }
        public decimal MontoOfertado { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ClienteId { get; set; } = string.Empty;
        public string ClienteName { get; set; } = string.Empty;
        public DateTime FechaOferta { get; set; }

        // Datos de la propiedad
        public int PropertyId { get; set; }
        public string PropertyCode { get; set; } = string.Empty;
        public decimal PropertyPrice { get; set; }
    }
}
