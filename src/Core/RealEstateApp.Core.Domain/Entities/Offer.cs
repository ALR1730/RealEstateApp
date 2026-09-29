using System;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Offer : AuditableBaseEntity
    {
        public decimal MontoOfertado { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Pending;
        public string ClienteId { get; set; } = string.Empty;
        public DateTime FechaOferta { get; set; } = DateTime.UtcNow;
        public string? PreApprovalLetterUrl { get; set; }
        public string? Notes { get; set; }

        // Campos de Contra-Oferta (Ítem 2.1)
        public decimal? CounterOfferAmount { get; set; }
        public string? CounterOfferMessage { get; set; }
        public DateTime? CounterOfferDate { get; set; }

        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        #region Domain Behaviors & Invariants

        /// <summary>
        /// Asigna el monto ofertado verificando que sea estrictamente positivo.
        /// </summary>
        public void SetOfferAmount(decimal amount)
        {
            if (amount <= 0)
            {
                throw new DomainException("El monto ofertado debe ser estrictamente mayor a cero.");
            }
            MontoOfertado = amount;
        }

        /// <summary>
        /// Acepta la propuesta de compra si se encuentra en estado pendiente.
        /// </summary>
        public void Accept()
        {
            if (Status != OfferStatus.Pending)
            {
                throw new DomainException($"Solo se pueden aceptar ofertas en estado pendiente. Estado actual: {Status}");
            }
            Status = OfferStatus.Accepted;
        }

        /// <summary>
        /// Rechaza la propuesta de compra si se encuentra en estado pendiente.
        /// </summary>
        public void Reject()
        {
            if (Status != OfferStatus.Pending)
            {
                throw new DomainException($"Solo se pueden rechazar ofertas en estado pendiente. Estado actual: {Status}");
            }
            Status = OfferStatus.Rejected;
        }

        /// <summary>
        /// Genera una contra-oferta sobre una propuesta pendiente.
        /// </summary>
        public void MakeCounterOffer(decimal counterAmount, string? counterMessage)
        {
            if (Status != OfferStatus.Pending)
            {
                throw new DomainException($"Solo se pueden realizar contra-ofertas sobre propuestas pendientes. Estado actual: {Status}");
            }
            if (counterAmount <= 0)
            {
                throw new DomainException("El monto de la contra-oferta debe ser mayor a cero.");
            }

            Status = OfferStatus.CounterOffered;
            CounterOfferAmount = counterAmount;
            CounterOfferMessage = counterMessage;
            CounterOfferDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Permite al cliente comprador aceptar formalmente una contra-oferta realizada por el agente.
        /// </summary>
        public void AcceptCounterOffer(string clientUserId)
        {
            if (Status != OfferStatus.CounterOffered)
            {
                throw new DomainException("Solo se pueden aceptar propuestas que tengan una contra-oferta activa.");
            }
            if (ClienteId != clientUserId)
            {
                throw new DomainException("No tiene permisos para modificar esta oferta.");
            }

            Status = OfferStatus.Accepted;
        }

        #endregion
    }
}
