using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class OfferServiceSecurityTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IUserActivityService> _userActivityServiceMock;
        private readonly Mock<ICommissionService> _commissionServiceMock;
        private readonly IOfferService _sut;

        public OfferServiceSecurityTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            var offerRepository = new OfferRepository(_dbContext);
            var propertyRepository = new PropertyRepository(_dbContext);
            var unitOfWork = new UnitOfWork(_dbContext);
            _userActivityServiceMock = new Mock<IUserActivityService>();
            _commissionServiceMock = new Mock<ICommissionService>();
            var mapper = AutoMapperTestFactory.CreateMapper();

            _sut = new OfferService(
                offerRepository,
                propertyRepository,
                _userActivityServiceMock.Object,
                _commissionServiceMock.Object,
                mapper,
                unitOfWork
            );
        }

        private async Task<(Property Property, Offer Offer)> SeedPropertyAndPendingOfferAsync(string ownerAgentId = "agent-owner")
        {
            var property = new Property
            {
                Name = "Apartamento Piantini",
                Code = "PIA901",
                Price = 6_000_000m,
                PriceInDOP = 6_000_000m,
                Currency = CurrencyConstants.DOP,
                AgentId = ownerAgentId
            };
            await _dbContext.Properties.AddAsync(property);
            await _dbContext.SaveChangesAsync();

            var offer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-buyer",
                MontoOfertado = 5_500_000m,
                FechaOferta = DateTime.UtcNow
            };
            await _dbContext.Offers.AddAsync(offer);
            await _dbContext.SaveChangesAsync();

            return (property, offer);
        }

        [Fact]
        public async Task AcceptOffer_DebeLanzarValidationExceptionYNoModificarEstado_CuandoAgenteNoEsPropietario()
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            Func<Task> act = () => _sut.AcceptOffer(offer.Id, "agent-attacker");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();

            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Pending, "un agente atacante no debe cambiar el estado de la oferta");

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available, "la propiedad debe permanecer Disponible");

            _commissionServiceMock.Verify(
                c => c.CreateForAcceptedOfferAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task RejectOffer_DebeLanzarValidationExceptionYNoModificarEstado_CuandoAgenteNoEsPropietario()
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            Func<Task> act = () => _sut.RejectOffer(offer.Id, "agent-attacker");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();

            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Pending, "un agente atacante no debe rechazar ofertas ajenas");

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);
        }

        [Fact]
        public async Task CounterOffer_DebeLanzarValidationExceptionYNoModificarEstado_CuandoAgenteNoEsPropietario()
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            Func<Task> act = () => _sut.CounterOffer(offer.Id, 5_000_000m, "msg", "agent-attacker");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();

            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Pending, "un agente atacante no debe generar contraofertas ajenas");
            offerInDb.CounterOfferAmount.Should().BeNull();
            offerInDb.CounterOfferMessage.Should().BeNull();

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AcceptOffer_DebeLanzarValidationException_CuandoAgentUserIdEsVacioONulo(string? invalidAgentId)
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            Func<Task> act = () => _sut.AcceptOffer(offer.Id, invalidAgentId!);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();

            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Pending);

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task RejectOffer_DebeLanzarValidationException_CuandoAgentUserIdEsVacioONulo(string? invalidAgentId)
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            Func<Task> act = () => _sut.RejectOffer(offer.Id, invalidAgentId!);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();

            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Pending);

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);
        }

        [Fact]
        public async Task AcceptOffer_DebeAceptarOfertaYVenderPropiedad_CuandoAgenteEsPropietarioLegitimo()
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            await _sut.AcceptOffer(offer.Id, "agent-owner");

            // Assert
            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Accepted);

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Sold);
        }

        [Fact]
        public async Task RejectOffer_DebeRechazarOferta_CuandoAgenteEsPropietarioLegitimo()
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            await _sut.RejectOffer(offer.Id, "agent-owner");

            // Assert
            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Rejected);

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);
        }

        [Fact]
        public async Task CounterOffer_DebeGenerarContraoferta_CuandoAgenteEsPropietarioLegitimo()
        {
            // Arrange
            var (property, offer) = await SeedPropertyAndPendingOfferAsync("agent-owner");

            // Act
            await _sut.CounterOffer(offer.Id, 5_800_000m, "Aceptamos 5.8M", "agent-owner");

            // Assert
            var offerInDb = await _dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.CounterOffered);
            offerInDb.CounterOfferAmount.Should().Be(5_800_000m);
            offerInDb.CounterOfferMessage.Should().Be("Aceptamos 5.8M");

            var propertyInDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
