using System;
using FluentAssertions;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;
using Xunit;

namespace RealEstateApp.UnitTests.Domain
{
    public class OfferDomainTests
    {
        [Fact]
        public void SetOfferAmount_DebeActualizarMonto_CuandoMontoEsPositivo()
        {
            // Arrange
            var offer = new Offer();

            // Act
            offer.SetOfferAmount(250000m);

            // Assert
            offer.MontoOfertado.Should().Be(250000m);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1000)]
        public void SetOfferAmount_DebeLanzarDomainException_CuandoMontoEsCeroONegativo(decimal montoInvalido)
        {
            // Arrange
            var offer = new Offer();

            // Act
            Action act = () => offer.SetOfferAmount(montoInvalido);

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("El monto ofertado debe ser estrictamente mayor a cero.");
        }

        [Fact]
        public void Accept_DebeCambiarEstadoAAccepted_CuandoOfertaEsPending()
        {
            // Arrange
            var offer = new Offer { Status = OfferStatus.Pending };

            // Act
            offer.Accept();

            // Assert
            offer.Status.Should().Be(OfferStatus.Accepted);
        }

        [Theory]
        [InlineData(OfferStatus.Accepted)]
        [InlineData(OfferStatus.Rejected)]
        [InlineData(OfferStatus.CounterOffered)]
        public void Accept_DebeLanzarDomainException_CuandoOfertaNoEsPending(OfferStatus estadoInvalido)
        {
            // Arrange
            var offer = new Offer { Status = estadoInvalido };

            // Act
            Action act = () => offer.Accept();

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage($"Solo se pueden aceptar ofertas en estado pendiente. Estado actual: {estadoInvalido}");
        }

        [Fact]
        public void Reject_DebeCambiarEstadoARejected_CuandoOfertaEsPending()
        {
            // Arrange
            var offer = new Offer { Status = OfferStatus.Pending };

            // Act
            offer.Reject();

            // Assert
            offer.Status.Should().Be(OfferStatus.Rejected);
        }

        [Theory]
        [InlineData(OfferStatus.Accepted)]
        [InlineData(OfferStatus.Rejected)]
        public void Reject_DebeLanzarDomainException_CuandoOfertaNoEsPending(OfferStatus estadoInvalido)
        {
            // Arrange
            var offer = new Offer { Status = estadoInvalido };

            // Act
            Action act = () => offer.Reject();

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage($"Solo se pueden rechazar ofertas en estado pendiente. Estado actual: {estadoInvalido}");
        }

        [Fact]
        public void MakeCounterOffer_DebeAsignarDatosYCambiarEstadoACounterOffered_CuandoOfertaEsPending()
        {
            // Arrange
            var offer = new Offer { Status = OfferStatus.Pending };

            // Act
            offer.MakeCounterOffer(280000m, "Monto mínimo aceptable");

            // Assert
            offer.Status.Should().Be(OfferStatus.CounterOffered);
            offer.CounterOfferAmount.Should().Be(280000m);
            offer.CounterOfferMessage.Should().Be("Monto mínimo aceptable");
            offer.CounterOfferDate.Should().NotBeNull();
            offer.CounterOfferDate!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public void MakeCounterOffer_DebeLanzarDomainException_CuandoMontoEsCeroONegativo(decimal montoInvalido)
        {
            // Arrange
            var offer = new Offer { Status = OfferStatus.Pending };

            // Act
            Action act = () => offer.MakeCounterOffer(montoInvalido, "Mensaje");

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("El monto de la contra-oferta debe ser mayor a cero.");
        }

        [Fact]
        public void MakeCounterOffer_DebeLanzarDomainException_CuandoOfertaNoEsPending()
        {
            // Arrange
            var offer = new Offer { Status = OfferStatus.Rejected };

            // Act
            Action act = () => offer.MakeCounterOffer(200000m, "Mensaje");

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage($"Solo se pueden realizar contra-ofertas sobre propuestas pendientes. Estado actual: {OfferStatus.Rejected}");
        }

        [Fact]
        public void AcceptCounterOffer_DebeCambiarEstadoAAccepted_CuandoUsuarioEsElClientePropietario()
        {
            // Arrange
            var offer = new Offer
            {
                ClienteId = "client-123",
                Status = OfferStatus.CounterOffered,
                CounterOfferAmount = 300000m
            };

            // Act
            offer.AcceptCounterOffer("client-123");

            // Assert
            offer.Status.Should().Be(OfferStatus.Accepted);
        }

        [Fact]
        public void AcceptCounterOffer_DebeLanzarDomainException_CuandoUsuarioNoEsElClientePropietario()
        {
            // Arrange
            var offer = new Offer
            {
                ClienteId = "client-123",
                Status = OfferStatus.CounterOffered
            };

            // Act
            Action act = () => offer.AcceptCounterOffer("impostor-456");

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("No tiene permisos para modificar esta oferta.");
        }

        [Fact]
        public void AcceptCounterOffer_DebeLanzarDomainException_CuandoOfertaNoTieneContraOfertaActiva()
        {
            // Arrange
            var offer = new Offer
            {
                ClienteId = "client-123",
                Status = OfferStatus.Pending
            };

            // Act
            Action act = () => offer.AcceptCounterOffer("client-123");

            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("Solo se pueden aceptar propuestas que tengan una contra-oferta activa.");
        }
    }
}
