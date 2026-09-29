using System;
using FluentAssertions;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
using Xunit;

namespace RealEstateApp.UnitTests.Domain
{
    public class PropertyDomainTests
    {
        [Fact]
        public void ChangePrice_DebeActualizarPrecioYSincronizarPriceInDOP_CuandoPrecioEsValidoEnDOP()
        {
            // Arrange
            var property = new Property { Price = 1000000m, Currency = CurrencyConstants.DOP };

            // Act
            property.ChangePrice(1500000m, CurrencyConstants.DOP);

            // Assert
            property.Price.Should().Be(1500000m);
            property.Currency.Should().Be(CurrencyConstants.DOP);
            property.PriceInDOP.Should().Be(1500000m);
        }

        [Fact]
        public void ChangePrice_DebeConvertirDOPCorrectamente_CuandoMonedaEsUSD()
        {
            // Arrange
            var property = new Property { Price = 100000m, Currency = CurrencyConstants.DOP };
            const decimal customRate = 60.5m;

            // Act
            property.ChangePrice(20000m, CurrencyConstants.USD, customRate);

            // Assert
            property.Price.Should().Be(20000m);
            property.Currency.Should().Be(CurrencyConstants.USD);
            property.PriceInDOP.Should().Be(20000m * customRate);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-500)]
        public void ChangePrice_DebeLanzarDomainException_CuandoPrecioEsCeroOMenor(decimal precioInvalido)
        {
            // Arrange
            var property = new Property { Price = 500000m };

            // Act
            Action act = () => property.ChangePrice(precioInvalido);

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("El precio de la propiedad debe ser estrictamente mayor a cero.");
        }

        [Fact]
        public void MarkAsAvailable_DebeLanzarDomainException_CuandoPropiedadYaFueVendida()
        {
            // Arrange
            var property = new Property { Status = PropertyStatus.Sold };

            // Act
            Action act = () => property.MarkAsAvailable();

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("No se puede marcar como disponible una propiedad que ya ha sido vendida.");
        }

        [Fact]
        public void MarkAsReserved_DebeLanzarDomainException_CuandoPropiedadYaFueVendida()
        {
            // Arrange
            var property = new Property { Status = PropertyStatus.Sold };

            // Act
            Action act = () => property.MarkAsReserved();

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("No se puede reservar una propiedad que ya ha sido vendida.");
        }

        [Fact]
        public void MarkAsReserved_DebeCambiarEstadoAReservada_CuandoPropiedadEstaDisponible()
        {
            // Arrange
            var property = new Property { Status = PropertyStatus.Available };

            // Act
            property.MarkAsReserved();

            // Assert
            property.Status.Should().Be(PropertyStatus.Reserved);
        }

        [Fact]
        public void MarkAsSold_DebeCambiarEstadoAVendida_CuandoPropiedadEstaDisponible()
        {
            // Arrange
            var property = new Property { Status = PropertyStatus.Available };

            // Act
            property.MarkAsSold();

            // Assert
            property.Status.Should().Be(PropertyStatus.Sold);
        }

        [Fact]
        public void PromoteToFeatured_DebeActivarDestacadoYCalcularFechaExpiracion()
        {
            // Arrange
            var property = new Property { IsFeatured = false };

            // Act
            property.PromoteToFeatured(15);

            // Assert
            property.IsFeatured.Should().BeTrue();
            property.FeaturedUntil.Should().NotBeNull();
            property.FeaturedUntil!.Value.Should().BeAfter(DateTime.UtcNow.AddDays(14));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void PromoteToFeatured_DebeLanzarDomainException_CuandoDuracionEsMenorOIgualACero(int dias)
        {
            // Arrange
            var property = new Property();

            // Act
            Action act = () => property.PromoteToFeatured(dias);

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("La duración del destacado debe ser de al menos 1 día.");
        }

        [Fact]
        public void RemoveFeatured_DebeDesactivarDestacadoYLimpiarFecha()
        {
            // Arrange
            var property = new Property
            {
                IsFeatured = true,
                FeaturedUntil = DateTime.UtcNow.AddDays(10)
            };

            // Act
            property.RemoveFeatured();

            // Assert
            property.IsFeatured.Should().BeFalse();
            property.FeaturedUntil.Should().BeNull();
        }

        [Fact]
        public void ReassignAgent_DebeActualizarAgentId_CuandoIdEsValido()
        {
            // Arrange
            var property = new Property { AgentId = "old-agent" };

            // Act
            property.ReassignAgent("new-agent");

            // Assert
            property.AgentId.Should().Be("new-agent");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void ReassignAgent_DebeLanzarDomainException_CuandoIdEsVacio(string? agenteInvalido)
        {
            // Arrange
            var property = new Property { AgentId = "old-agent" };

            // Act
            Action act = () => property.ReassignAgent(agenteInvalido!);

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("El ID del nuevo agente no puede ser vacío o nulo.");
        }
    }
}
