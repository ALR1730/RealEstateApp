using System;
using System.Threading.Tasks;
using AutoMapper;
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

namespace RealEstateApp.UnitTests.Persistence
{
    public class OptimisticConcurrencyTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Property_DebeInicializarRowVersionEnUno_YIncrementarAlActualizar()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            int propertyId;

            await using (var setupCtx = CreateContext(dbName))
            {
                var property = new Property
                {
                    Name = "Torre Coral",
                    Code = "COR101",
                    Price = 7_500_000m,
                    PriceInDOP = 7_500_000m,
                    Currency = CurrencyConstants.DOP,
                    AgentId = "agent-1"
                };

                await setupCtx.Properties.AddAsync(property);
                await setupCtx.SaveChangesAsync();
                property.RowVersion.Should().Be(1);
                propertyId = property.Id;
            }

            // Act
            await using (var updateCtx = CreateContext(dbName))
            {
                var repo = new PropertyRepository(updateCtx);
                var loaded = await repo.FindByIdAsync(propertyId);
                loaded!.ChangePrice(8_000_000m, CurrencyConstants.DOP);
                await repo.UpdateAsync(loaded);

                loaded.RowVersion.Should().Be(2);
            }

            // Assert
            await using var verifyCtx = CreateContext(dbName);
            var persisted = await verifyCtx.Properties.FindAsync(propertyId);
            persisted!.RowVersion.Should().Be(2);
            persisted.Price.Should().Be(8_000_000m);
        }

        [Fact]
        public async Task Offer_DebeInicializarRowVersionEnUno_YIncrementarAlActualizar()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            int offerId;

            await using (var setupCtx = CreateContext(dbName))
            {
                var property = new Property
                {
                    Name = "Villa Palma",
                    Code = "PAL202",
                    Price = 12_000_000m,
                    PriceInDOP = 12_000_000m,
                    Currency = CurrencyConstants.DOP,
                    AgentId = "agent-1"
                };
                await setupCtx.Properties.AddAsync(property);
                await setupCtx.SaveChangesAsync();

                var offer = new Offer
                {
                    PropertyId = property.Id,
                    ClienteId = "client-1",
                    MontoOfertado = 11_000_000m,
                    FechaOferta = DateTime.UtcNow
                };
                await setupCtx.Offers.AddAsync(offer);
                await setupCtx.SaveChangesAsync();

                offer.RowVersion.Should().Be(1);
                offerId = offer.Id;
            }

            // Act
            await using (var updateCtx = CreateContext(dbName))
            {
                var offerRepo = new OfferRepository(updateCtx);
                var loaded = await offerRepo.GetByIdAsync(offerId);
                loaded!.MakeCounterOffer(11_500_000m, "Contraoferta");
                await offerRepo.UpdateAsync(loaded);

                loaded.RowVersion.Should().Be(2);
            }

            // Assert
            await using var verifyCtx = CreateContext(dbName);
            var persisted = await verifyCtx.Offers.FindAsync(offerId);
            persisted!.RowVersion.Should().Be(2);
            persisted.Status.Should().Be(OfferStatus.CounterOffered);
        }

        [Fact]
        public async Task UpdateProperty_DebeLanzarDbUpdateConcurrencyException_CuandoDosContextosModificanConMismoRowVersion()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            int propertyId;

            await using (var setupCtx = CreateContext(dbName))
            {
                var property = new Property
                {
                    Name = "Apartamento Naco",
                    Code = "NAC303",
                    Price = 5_000_000m,
                    PriceInDOP = 5_000_000m,
                    Currency = CurrencyConstants.DOP,
                    AgentId = "agent-1"
                };
                await setupCtx.Properties.AddAsync(property);
                await setupCtx.SaveChangesAsync();
                propertyId = property.Id;
            }

            await using var ctx1 = CreateContext(dbName);
            await using var ctx2 = CreateContext(dbName);

            var repo1 = new PropertyRepository(ctx1);
            var repo2 = new PropertyRepository(ctx2);

            var prop1 = await repo1.FindByIdAsync(propertyId);
            var prop2 = await repo2.FindByIdAsync(propertyId);

            prop1!.RowVersion.Should().Be(1);
            prop2!.RowVersion.Should().Be(1);

            prop1.ChangePrice(5_500_000m, CurrencyConstants.DOP);
            await repo1.UpdateAsync(prop1);

            // Act
            prop2.ChangePrice(6_000_000m, CurrencyConstants.DOP);
            Func<Task> act = () => repo2.UpdateAsync(prop2);

            // Assert
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();

            await using var verifyCtx = CreateContext(dbName);
            var persisted = await verifyCtx.Properties.FindAsync(propertyId);
            persisted!.Price.Should().Be(5_500_000m);
            persisted.RowVersion.Should().Be(2);
        }

        [Fact]
        public async Task AcceptOffer_DebeLanzarDbUpdateConcurrencyExceptionYSinDobleComision_CuandoDosContextosConcurrenSobreMismaPropiedad()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            int propertyId;
            int offer1Id;
            int offer2Id;

            await using (var setupCtx = CreateContext(dbName))
            {
                var property = new Property
                {
                    Name = "Penthouse Evaristo Morales",
                    Code = "EVA404",
                    Price = 10_000_000m,
                    PriceInDOP = 10_000_000m,
                    Currency = CurrencyConstants.DOP,
                    AgentId = "agent-1"
                };
                await setupCtx.Properties.AddAsync(property);
                await setupCtx.SaveChangesAsync();

                var offer1 = new Offer
                {
                    PropertyId = property.Id,
                    ClienteId = "client-1",
                    MontoOfertado = 9_800_000m,
                    FechaOferta = DateTime.UtcNow
                };
                var offer2 = new Offer
                {
                    PropertyId = property.Id,
                    ClienteId = "client-2",
                    MontoOfertado = 9_700_000m,
                    FechaOferta = DateTime.UtcNow
                };
                await setupCtx.Offers.AddRangeAsync(offer1, offer2);
                await setupCtx.SaveChangesAsync();

                propertyId = property.Id;
                offer1Id = offer1.Id;
                offer2Id = offer2.Id;
            }

            await using var ctx1 = CreateContext(dbName);
            await using var ctx2 = CreateContext(dbName);

            var offerRepo1 = new OfferRepository(ctx1);
            var propRepo1 = new PropertyRepository(ctx1);
            var uow1 = new UnitOfWork(ctx1);
            var commissionMock1 = new Mock<ICommissionService>();
            var service1 = new OfferService(
                offerRepo1,
                propRepo1,
                new Mock<IUserActivityService>().Object,
                commissionMock1.Object,
                AutoMapperTestFactory.CreateMapper(),
                uow1);

            var offerRepo2 = new OfferRepository(ctx2);
            var propRepo2 = new PropertyRepository(ctx2);
            var uow2 = new UnitOfWork(ctx2);
            var commissionMock2 = new Mock<ICommissionService>();
            var service2 = new OfferService(
                offerRepo2,
                propRepo2,
                new Mock<IUserActivityService>().Object,
                commissionMock2.Object,
                AutoMapperTestFactory.CreateMapper(),
                uow2);

            // Contexto B precarga estado concurrente antes del commit del Contexto A
            _ = await offerRepo2.GetByIdAsync(offer2Id);
            _ = await propRepo2.FindByIdAsync(propertyId);

            // Contexto A cierra la propiedad aceptando oferta 1
            await service1.AcceptOffer(offer1Id, "agent-1");

            // Act - Contexto B intenta cerrar simultáneamente la oferta 2 con RowVersion obsoleto
            Func<Task> act = () => service2.AcceptOffer(offer2Id, "agent-1");

            // Assert
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
            commissionMock1.Verify(c => c.CreateForAcceptedOfferAsync(offer1Id), Times.Once);
            commissionMock2.Verify(c => c.CreateForAcceptedOfferAsync(It.IsAny<int>()), Times.Never);

            await using var verifyCtx = CreateContext(dbName);
            var persistedProperty = await verifyCtx.Properties.FindAsync(propertyId);
            var persistedOffer1 = await verifyCtx.Offers.FindAsync(offer1Id);
            var persistedOffer2 = await verifyCtx.Offers.FindAsync(offer2Id);

            persistedProperty!.Status.Should().Be(PropertyStatus.Sold);
            persistedProperty.RowVersion.Should().Be(2);
            persistedOffer1!.Status.Should().Be(OfferStatus.Accepted);
            persistedOffer2!.Status.Should().Be(OfferStatus.Rejected);
        }
    }
}
