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
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    /// <summary>
    /// Pruebas basadas en Estado y Comportamiento Real (Social Testing - Anti-Mockitis).
    /// En lugar de verificar llamadas a Mocks intermediarios, estas pruebas configuran
    /// un contexto EF Core real y asertan directamente sobre las mutaciones en la Base de Datos.
    /// </summary>
    public class OfferServiceStateTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OfferService _sut;

        public OfferServiceStateTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            var offerRepository = new OfferRepository(_dbContext);
            var propertyRepository = new PropertyRepository(_dbContext);
            var unitOfWork = new UnitOfWork(_dbContext);
            var userActivityServiceMock = new Mock<IUserActivityService>();
            var commissionServiceMock = new Mock<ICommissionService>();
            var mapper = AutoMapperTestFactory.CreateMapper();

            _sut = new OfferService(
                offerRepository,
                propertyRepository,
                userActivityServiceMock.Object,
                commissionServiceMock.Object,
                mapper,
                unitOfWork
            );
        }

        [Fact]
        public async Task AcceptOffer_DebeActualizarEstadoEnBaseDeDatos_AceptandoOferta_VendiendoPropiedad_YRechazandoOtrasOfertas()
        {
            // Arrange - Estado inicial real en la BD
            var property = new Property
            {
                Name = "Villa Frente al Mar",
                Code = "VIL101",
                Price = 5000000m,
                PriceInDOP = 5000000m,
                Currency = CurrencyConstants.DOP,
                Status = PropertyStatus.Available,
                AgentId = "agent-owner"
            };
            await _dbContext.Properties.AddAsync(property);
            await _dbContext.SaveChangesAsync();

            var otherProperty = new Property
            {
                Name = "Apartamento Centro",
                Code = "APT202",
                Price = 3000000m,
                PriceInDOP = 3000000m,
                Currency = CurrencyConstants.DOP,
                Status = PropertyStatus.Available,
                AgentId = "agent-owner"
            };
            await _dbContext.Properties.AddAsync(otherProperty);
            await _dbContext.SaveChangesAsync();

            // Oferta a aceptar para la propiedad 1
            var offerToAccept = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-winner",
                MontoOfertado = 4800000m,
                Status = OfferStatus.Pending,
                FechaOferta = DateTime.UtcNow
            };

            // Oferta competidora para la misma propiedad (debe ser rechazada)
            var competingOffer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-loser",
                MontoOfertado = 4700000m,
                Status = OfferStatus.Pending,
                FechaOferta = DateTime.UtcNow
            };

            // Oferta para otra propiedad diferente (debe permanecer pendiente intacta)
            var unrelatedOffer = new Offer
            {
                PropertyId = otherProperty.Id,
                ClienteId = "client-other",
                MontoOfertado = 2900000m,
                Status = OfferStatus.Pending,
                FechaOferta = DateTime.UtcNow
            };

            await _dbContext.Offers.AddRangeAsync(offerToAccept, competingOffer, unrelatedOffer);
            await _dbContext.SaveChangesAsync();

            // Act - Ejecución real del caso de uso
            await _sut.AcceptOffer(offerToAccept.Id);

            // Assert - Verificación de Estado observable directamente en la Base de Datos
            var propertyEnDb = await _dbContext.Properties.FindAsync(property.Id);
            propertyEnDb!.Status.Should().Be(PropertyStatus.Sold, "la propiedad debe quedar marcada como Vendida");

            var acceptedOfferEnDb = await _dbContext.Offers.FindAsync(offerToAccept.Id);
            acceptedOfferEnDb!.Status.Should().Be(OfferStatus.Accepted, "la oferta ganadora debe quedar marcada como Aceptada");

            var competingOfferEnDb = await _dbContext.Offers.FindAsync(competingOffer.Id);
            competingOfferEnDb!.Status.Should().Be(OfferStatus.Rejected, "las ofertas competidoras de la misma propiedad deben rechazarse en cascada");

            var unrelatedOfferEnDb = await _dbContext.Offers.FindAsync(unrelatedOffer.Id);
            unrelatedOfferEnDb!.Status.Should().Be(OfferStatus.Pending, "las ofertas sobre otras propiedades NO deben ser afectadas");
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
