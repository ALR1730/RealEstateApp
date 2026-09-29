using System.Collections.Generic;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Property : AuditableBaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = CurrencyConstants.DOP;
        public decimal PriceInDOP { get; set; }
        public int Rooms { get; set; }
        public int Bathrooms { get; set; }
        public decimal SizeInMeters { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AgentId { get; set; } = string.Empty;
        public string Status { get; set; } = PropertyStatus.Available;

        // Nuevos campos de mejoras funcionales V2
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? VideoUrl { get; set; }
        public string? Tour360Url { get; set; }
        public string? MatterportModelId { get; set; }
        public decimal MontoSeparacion { get; set; }
        public int PorcentajeInicialRequerido { get; set; }
        public bool IsFinanciable { get; set; } = false;

        // Listados Destacados (Featured)
        public bool IsFeatured { get; set; } = false;
        public System.DateTime? FeaturedUntil { get; set; }

        // Ubicación administrativa en República Dominicana
        public int? ProvinceId { get; set; }
        public Province? Province { get; set; }

        public int? MunicipalityId { get; set; }
        public Municipality? Municipality { get; set; }

        public string? Sector { get; set; }
        public string? FullAddress { get; set; }

        // Relaciones
        public int PropertyTypeId { get; set; }
        public PropertyType? PropertyType { get; set; }

        public int SaleTypeId { get; set; }
        public SaleType? SaleType { get; set; }

        public ICollection<PropertyImage>? Images { get; set; }
        public ICollection<PropertyImprovement>? PropertyImprovements { get; set; }
        public ICollection<Offer>? Offers { get; set; }
        public ICollection<Chat>? Chats { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
        public ICollection<MortgageSimulation>? MortgageSimulations { get; set; }
        public ICollection<PropertyAppointment>? Appointments { get; set; }
        public ICollection<PropertyPriceHistory>? PriceHistories { get; set; }

        #region Domain Behaviors & Invariants

        /// <summary>
        /// Modifica el precio de la propiedad asegurando que sea mayor a cero y sincronizando el precio en DOP.
        /// </summary>
        public void ChangePrice(decimal newPrice, string? currency = null, decimal? customUsdRate = null)
        {
            if (newPrice <= 0)
            {
                throw new DomainException("El precio de la propiedad debe ser estrictamente mayor a cero.");
            }

            Price = newPrice;

            if (!string.IsNullOrWhiteSpace(currency))
            {
                Currency = currency.ToUpperInvariant();
            }

            var rate = customUsdRate ?? CurrencyConstants.DefaultUsdToDopRate;

            if (string.Equals(Currency, CurrencyConstants.USD, System.StringComparison.OrdinalIgnoreCase))
            {
                PriceInDOP = newPrice * rate;
            }
            else
            {
                PriceInDOP = newPrice;
            }
        }

        /// <summary>
        /// Marca la propiedad como disponible. No permite transicionar propiedades vendidas directamente.
        /// </summary>
        public void MarkAsAvailable()
        {
            if (Status == PropertyStatus.Sold)
            {
                throw new DomainException("No se puede marcar como disponible una propiedad que ya ha sido vendida.");
            }
            Status = PropertyStatus.Available;
        }

        /// <summary>
        /// Marca la propiedad como reservada.
        /// </summary>
        public void MarkAsReserved()
        {
            if (Status == PropertyStatus.Sold)
            {
                throw new DomainException("No se puede reservar una propiedad que ya ha sido vendida.");
            }
            Status = PropertyStatus.Reserved;
        }

        /// <summary>
        /// Marca la propiedad como vendida.
        /// </summary>
        public void MarkAsSold()
        {
            Status = PropertyStatus.Sold;
        }

        /// <summary>
        /// Promueve la propiedad al estado de destacada por una cantidad determinada de días.
        /// </summary>
        public void PromoteToFeatured(int durationDays = 30)
        {
            if (durationDays <= 0)
            {
                throw new DomainException("La duración del destacado debe ser de al menos 1 día.");
            }

            IsFeatured = true;
            FeaturedUntil = System.DateTime.UtcNow.AddDays(durationDays);
        }

        /// <summary>
        /// Remueve el estado de destacada de la propiedad.
        /// </summary>
        public void RemoveFeatured()
        {
            IsFeatured = false;
            FeaturedUntil = null;
        }

        /// <summary>
        /// Reasigna la propiedad a un nuevo agente responsable.
        /// </summary>
        public void ReassignAgent(string newAgentId)
        {
            if (string.IsNullOrWhiteSpace(newAgentId))
            {
                throw new DomainException("El ID del nuevo agente no puede ser vacío o nulo.");
            }

            AgentId = newAgentId;
        }

        #endregion
    }
}
