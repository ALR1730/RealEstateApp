using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.BuyAbility;
using RealEstateApp.Core.Application.ViewModels.Property;
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
    public class OfferAndOwnerTransactionsTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static OfferService CreateOfferService(
            ApplicationDbContext dbContext,
            out Mock<IUserActivityService> userActivityServiceMock,
            out Mock<ICommissionService> commissionServiceMock)
        {
            var offerRepository = new OfferRepository(dbContext);
            var propertyRepository = new PropertyRepository(dbContext);
            var unitOfWork = new UnitOfWork(dbContext);
            userActivityServiceMock = new Mock<IUserActivityService>();
            commissionServiceMock = new Mock<ICommissionService>();
            IMapper mapper = AutoMapperTestFactory.CreateMapper();

            return new OfferService(
                offerRepository,
                propertyRepository,
                userActivityServiceMock.Object,
                commissionServiceMock.Object,
                mapper,
                unitOfWork);
        }

        #region OfferService

        [Fact]
        public void OfferService_DebeLanzarArgumentNullException_CuandoUnitOfWorkEsNulo()
        {
            // Arrange & Act
            Action act = () => _ = new OfferService(
                new Mock<IOfferRepository>().Object,
                new Mock<IPropertyRepository>().Object,
                new Mock<IUserActivityService>().Object,
                new Mock<ICommissionService>().Object,
                AutoMapperTestFactory.CreateMapper(),
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task AcceptCounterOffer_DebeEjecutarTransicionAtomicaYRegistrarComision_CuandoContraOfertaEsValida()
        {
            // Arrange
            using var dbContext = CreateInMemoryContext();
            var sut = CreateOfferService(dbContext, out _, out var commissionServiceMock);

            var property = new Property
            {
                Name = "Penthouse Piantini",
                Code = "PEN101",
                Price = 10_000_000m,
                PriceInDOP = 10_000_000m,
                Currency = CurrencyConstants.DOP,
                AgentId = "agent-1"
            };
            await dbContext.Properties.AddAsync(property);
            await dbContext.SaveChangesAsync();

            var winningCounterOffer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-winner",
                MontoOfertado = 9_000_000m,
                FechaOferta = DateTime.UtcNow
            };
            winningCounterOffer.MakeCounterOffer(9_500_000m, "Aceptamos cerrar en 9.5M");

            var competingPendingOffer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-pending",
                MontoOfertado = 8_700_000m,
                FechaOferta = DateTime.UtcNow
            };

            var competingCounterOffer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-counter",
                MontoOfertado = 8_800_000m,
                FechaOferta = DateTime.UtcNow
            };
            competingCounterOffer.MakeCounterOffer(9_400_000m, "Otra contraoferta");

            await dbContext.Offers.AddRangeAsync(winningCounterOffer, competingPendingOffer, competingCounterOffer);
            await dbContext.SaveChangesAsync();

            // Act
            await sut.AcceptCounterOffer(winningCounterOffer.Id, "client-winner");

            // Assert
            var acceptedInDb = await dbContext.Offers.FindAsync(winningCounterOffer.Id);
            acceptedInDb!.Status.Should().Be(OfferStatus.Accepted);

            var propertyInDb = await dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Sold);

            var pendingInDb = await dbContext.Offers.FindAsync(competingPendingOffer.Id);
            pendingInDb!.Status.Should().Be(OfferStatus.Rejected);

            var counterInDb = await dbContext.Offers.FindAsync(competingCounterOffer.Id);
            counterInDb!.Status.Should().Be(OfferStatus.Rejected);

            commissionServiceMock.Verify(
                c => c.CreateForAcceptedOfferAsync(winningCounterOffer.Id),
                Times.Once);
        }

        [Fact]
        public async Task AcceptCounterOffer_DebeLanzarValidationExceptionSinModificarBD_CuandoClienteEsDistinto()
        {
            // Arrange
            using var dbContext = CreateInMemoryContext();
            var sut = CreateOfferService(dbContext, out _, out var commissionServiceMock);

            var property = new Property
            {
                Name = "Apartamento Gazcue",
                Code = "GAZ202",
                Price = 6_000_000m,
                PriceInDOP = 6_000_000m,
                Currency = CurrencyConstants.DOP,
                AgentId = "agent-1"
            };
            await dbContext.Properties.AddAsync(property);
            await dbContext.SaveChangesAsync();

            var offer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-owner",
                MontoOfertado = 5_500_000m,
                FechaOferta = DateTime.UtcNow
            };
            offer.MakeCounterOffer(5_800_000m, "Contraoferta");
            await dbContext.Offers.AddAsync(offer);
            await dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await sut.AcceptCounterOffer(offer.Id, "client-intruder");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();

            var offerInDb = await dbContext.Offers.FindAsync(offer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.CounterOffered);

            var propertyInDb = await dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);

            commissionServiceMock.Verify(
                c => c.CreateForAcceptedOfferAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task AcceptCounterOffer_DebeLanzarValidationExceptionSinModificarBD_CuandoEstadoNoEsCounterOffered()
        {
            // Arrange
            using var dbContext = CreateInMemoryContext();
            var sut = CreateOfferService(dbContext, out _, out var commissionServiceMock);

            var property = new Property
            {
                Name = "Casa Arroyo Hondo",
                Code = "ARR303",
                Price = 8_000_000m,
                PriceInDOP = 8_000_000m,
                Currency = CurrencyConstants.DOP,
                AgentId = "agent-1"
            };
            await dbContext.Properties.AddAsync(property);
            await dbContext.SaveChangesAsync();

            var pendingOffer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-1",
                MontoOfertado = 7_500_000m,
                FechaOferta = DateTime.UtcNow
            };
            await dbContext.Offers.AddAsync(pendingOffer);
            await dbContext.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await sut.AcceptCounterOffer(pendingOffer.Id, "client-1");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();

            var offerInDb = await dbContext.Offers.FindAsync(pendingOffer.Id);
            offerInDb!.Status.Should().Be(OfferStatus.Pending);

            var propertyInDb = await dbContext.Properties.FindAsync(property.Id);
            propertyInDb!.Status.Should().Be(PropertyStatus.Available);

            commissionServiceMock.Verify(
                c => c.CreateForAcceptedOfferAsync(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task AcceptOffer_DebePropagarExcepcionSinOcultarla_CuandoCreateForAcceptedOfferFallaDentroDeUnitOfWork()
        {
            // Arrange
            using var dbContext = CreateInMemoryContext();
            var sut = CreateOfferService(dbContext, out _, out var commissionServiceMock);

            var property = new Property
            {
                Name = "Villa Bavaro",
                Code = "BAV404",
                Price = 9_000_000m,
                PriceInDOP = 9_000_000m,
                Currency = CurrencyConstants.DOP,
                AgentId = "agent-1"
            };
            await dbContext.Properties.AddAsync(property);
            await dbContext.SaveChangesAsync();

            var offer = new Offer
            {
                PropertyId = property.Id,
                ClienteId = "client-1",
                MontoOfertado = 8_800_000m,
                FechaOferta = DateTime.UtcNow
            };
            await dbContext.Offers.AddAsync(offer);
            await dbContext.SaveChangesAsync();

            commissionServiceMock
                .Setup(c => c.CreateForAcceptedOfferAsync(offer.Id))
                .ThrowsAsync(new InvalidOperationException("Fallo calculando comisión"));

            // Act
            Func<Task> act = async () => await sut.AcceptOffer(offer.Id, "agent-1");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Fallo calculando comisión");
        }

        #endregion

        #region BuyAbilityService

        [Fact]
        public void BuyAbilityService_DebeLanzarArgumentNullException_CuandoUnitOfWorkEsNulo()
        {
            // Arrange & Act
            Action act = () => _ = new BuyAbilityService(
                new Mock<IBuyAbilityEvaluationRepository>().Object,
                new Mock<ICurrencyService>().Object,
                AutoMapperTestFactory.CreateMapper(),
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task EvaluateAsync_DebeEjecutarDentroDeUnitOfWorkYDesactivarEvaluacionesPrevias()
        {
            // Arrange
            var repoMock = new Mock<IBuyAbilityEvaluationRepository>();
            var currencyServiceMock = new Mock<ICurrencyService>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            IMapper mapper = AutoMapperTestFactory.CreateMapper();

            var previousEvaluation1 = new BuyAbilityEvaluation
            {
                Id = 1,
                ClientId = "client-1",
                IsActive = true
            };
            var previousEvaluation2 = new BuyAbilityEvaluation
            {
                Id = 2,
                ClientId = "client-1",
                IsActive = true
            };
            var otherClientEvaluation = new BuyAbilityEvaluation
            {
                Id = 3,
                ClientId = "client-other",
                IsActive = true
            };

            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<BuyAbilityEvaluation>
            {
                previousEvaluation1,
                previousEvaluation2,
                otherClientEvaluation
            });
            repoMock.Setup(r => r.AddAsync(It.IsAny<BuyAbilityEvaluation>()))
                .ReturnsAsync((BuyAbilityEvaluation evaluation) =>
                {
                    evaluation.Id = 99;
                    return evaluation;
                });

            unitOfWorkMock
                .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default))
                .Returns<Func<Task>, CancellationToken>((action, _) => action());

            var sut = new BuyAbilityService(
                repoMock.Object,
                currencyServiceMock.Object,
                mapper,
                unitOfWorkMock.Object);

            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 75_000m,
                MonthlyNetIncome = 60_000m,
                MonthlyDebtPayments = 5_000m,
                AvailableDownPayment = 400_000m,
                Currency = CurrencyConstants.DOP
            };

            // Act
            var result = await sut.EvaluateAsync("client-1", request);

            // Assert
            result.Should().NotBeNull();
            unitOfWorkMock.Verify(
                u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default),
                Times.Once);

            previousEvaluation1.IsActive.Should().BeFalse();
            previousEvaluation2.IsActive.Should().BeFalse();
            otherClientEvaluation.IsActive.Should().BeTrue();

            repoMock.Verify(
                r => r.AddAsync(It.Is<BuyAbilityEvaluation>(e => e.ClientId == "client-1" && e.IsActive)),
                Times.Once);
        }

        #endregion

        #region PropertyService (Owner Limits)

        private static PropertyService CreatePropertyService(
            out Mock<IPropertyRepository> propertyRepositoryMock,
            out Mock<IPropertyTypeRepository> propertyTypeRepositoryMock,
            out Mock<IPropertyPriceHistoryRepository> priceHistoryRepositoryMock,
            out Mock<ICurrencyService> currencyServiceMock,
            out Mock<ISubscriptionService> subscriptionServiceMock)
        {
            propertyRepositoryMock = new Mock<IPropertyRepository>();
            var propertyImageRepositoryMock = new Mock<IPropertyImageRepository>();
            propertyTypeRepositoryMock = new Mock<IPropertyTypeRepository>();
            var propertyImprovementRepositoryMock = new Mock<IPropertyImprovementRepository>();
            priceHistoryRepositoryMock = new Mock<IPropertyPriceHistoryRepository>();
            var fileStorageServiceMock = new Mock<IFileStorageService>();
            currencyServiceMock = new Mock<ICurrencyService>();
            var savedSearchServiceMock = new Mock<ISavedSearchService>();
            subscriptionServiceMock = new Mock<ISubscriptionService>();
            var accountServiceMock = new Mock<IAccountService>();
            IMapper mapper = AutoMapperTestFactory.CreateMapper();

            currencyServiceMock.Setup(c => c.GetExchangeRateAsync()).ReturnsAsync(60.0m);
            currencyServiceMock
                .Setup(c => c.FormatPrice(It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns<decimal, string>((price, curr) => $"{curr} {price:N2}");
            currencyServiceMock
                .Setup(c => c.FormatSecondaryPrice(
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal>()))
                .Returns("USD $0.00");

            subscriptionServiceMock
                .Setup(s => s.CanAgentCreatePropertyAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            propertyTypeRepositoryMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new PropertyType { Id = 1, Name = "Apartamento" });

            priceHistoryRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()))
                .ReturnsAsync((PropertyPriceHistory h) => h);

            return new PropertyService(
                propertyRepositoryMock.Object,
                propertyImageRepositoryMock.Object,
                propertyTypeRepositoryMock.Object,
                propertyImprovementRepositoryMock.Object,
                priceHistoryRepositoryMock.Object,
                fileStorageServiceMock.Object,
                currencyServiceMock.Object,
                savedSearchServiceMock.Object,
                subscriptionServiceMock.Object,
                accountServiceMock.Object,
                mapper);
        }

        [Fact]
        public async Task AddForOwnerAsync_DebePermitirCrearPropiedadYAsignarOwner_CuandoTieneMenosDelLimite()
        {
            // Arrange
            var sut = CreatePropertyService(
                out var propertyRepositoryMock,
                out _,
                out _,
                out _,
                out _);

            propertyRepositoryMock
                .Setup(r => r.GetByAgentIdAsync("owner-1"))
                .ReturnsAsync(new List<Property>
                {
                    new Property { Id = 1, Name = "Propiedad Existente", AgentId = "owner-1" }
                });

            propertyRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Property>()))
                .ReturnsAsync((Property p) =>
                {
                    p.Id = 25;
                    return p;
                });

            var vm = new SavePropertyViewModel
            {
                Name = "Apartamento en Bella Vista",
                Price = 4_500_000m,
                Currency = CurrencyConstants.DOP,
                Rooms = 3,
                Bathrooms = 2,
                SizeInMeters = 140m,
                Description = "Excelente ubicación",
                PropertyTypeId = 1,
                SaleTypeId = 1,
                AgentId = string.Empty
            };

            // Act
            var result = await sut.AddForOwnerAsync(vm, "owner-1");

            // Assert
            result.Should().NotBeNull();
            result.AgentId.Should().Be("owner-1");
            propertyRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Property>(p => p.AgentId == "owner-1")),
                Times.Once);
        }

        [Fact]
        public async Task AddForOwnerAsync_DebeLanzarValidationExceptionYNoGuardar_CuandoAlcanzaLimiteDePropiedadesOwner()
        {
            // Arrange
            PropertyLimits.MaxOwnerProperties.Should().Be(2);

            var sut = CreatePropertyService(
                out var propertyRepositoryMock,
                out _,
                out _,
                out _,
                out _);

            propertyRepositoryMock
                .Setup(r => r.GetByAgentIdAsync("owner-1"))
                .ReturnsAsync(new List<Property>
                {
                    new Property { Id = 1, Name = "Propiedad 1", AgentId = "owner-1" },
                    new Property { Id = 2, Name = "Propiedad 2", AgentId = "owner-1" }
                });

            var vm = new SavePropertyViewModel
            {
                Name = "Tercera Propiedad No Permitida",
                Price = 5_000_000m,
                Currency = CurrencyConstants.DOP,
                Rooms = 3,
                Bathrooms = 2,
                SizeInMeters = 150m,
                Description = "Excede el límite permitido para propietarios",
                PropertyTypeId = 1,
                SaleTypeId = 1
            };

            // Act
            Func<Task> act = async () => await sut.AddForOwnerAsync(vm, "owner-1");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
            propertyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Property>()), Times.Never);
        }

        #endregion
    }
}
