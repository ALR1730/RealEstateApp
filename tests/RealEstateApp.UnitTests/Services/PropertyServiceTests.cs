using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class PropertyServiceTests
    {
        private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
        private readonly Mock<IPropertyImageRepository> _propertyImageRepositoryMock;
        private readonly Mock<IPropertyTypeRepository> _propertyTypeRepositoryMock;
        private readonly Mock<IPropertyImprovementRepository> _propertyImprovementRepositoryMock;
        private readonly Mock<IPropertyPriceHistoryRepository> _priceHistoryRepositoryMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly Mock<ISavedSearchService> _savedSearchServiceMock;
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly IMapper _mapper;
        private readonly PropertyService _sut; // System Under Test

        public PropertyServiceTests()
        {
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _propertyImageRepositoryMock = new Mock<IPropertyImageRepository>();
            _propertyTypeRepositoryMock = new Mock<IPropertyTypeRepository>();
            _propertyImprovementRepositoryMock = new Mock<IPropertyImprovementRepository>();
            _priceHistoryRepositoryMock = new Mock<IPropertyPriceHistoryRepository>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _savedSearchServiceMock = new Mock<ISavedSearchService>();
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _accountServiceMock = new Mock<IAccountService>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            // Default currency service mocks
            _currencyServiceMock.Setup(c => c.GetExchangeRateAsync()).ReturnsAsync(60.0m);
            _currencyServiceMock.Setup(c => c.FormatPrice(It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns<decimal, string>((price, curr) => $"{curr} {price:N2}");
            _currencyServiceMock.Setup(c => c.FormatSecondaryPrice(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns("USD $0.00");

            _subscriptionServiceMock.Setup(s => s.CanAgentCreatePropertyAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _sut = new PropertyService(
                _propertyRepositoryMock.Object,
                _propertyImageRepositoryMock.Object,
                _propertyTypeRepositoryMock.Object,
                _propertyImprovementRepositoryMock.Object,
                _priceHistoryRepositoryMock.Object,
                _fileStorageServiceMock.Object,
                _currencyServiceMock.Object,
                _savedSearchServiceMock.Object,
                _subscriptionServiceMock.Object,
                _accountServiceMock.Object,
                _mapper
            );
        }

        #region Feature F-02: Price History & Price Drop Tests

        [Fact]
        public async Task Add_ShouldRecordInitialPriceHistory_WhenPropertyIsCreated()
        {
            // Arrange
            var vm = new SavePropertyViewModel
            {
                Name = "Villa Moderna",
                Price = 250000m,
                Currency = CurrencyConstants.USD,
                PropertyTypeId = 1,
                SaleTypeId = 1,
                AgentId = "agent-123",
                Rooms = 4,
                Bathrooms = 3,
                SizeInMeters = 350
            };

            _propertyTypeRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new PropertyType { Id = 1, Name = "Villa" });

            _propertyRepositoryMock.Setup(r => r.GetByAgentIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Property>());

            _propertyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Property>()))
                .ReturnsAsync((Property p) => { p.Id = 10; return p; });

            PropertyPriceHistory? capturedHistory = null;
            _priceHistoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()))
                .Callback<PropertyPriceHistory>(h => capturedHistory = h)
                .ReturnsAsync((PropertyPriceHistory h) => h);

            // Act
            var result = await _sut.Add(vm);

            // Assert
            result.Should().NotBeNull();
            _priceHistoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()), Times.Once);
            capturedHistory.Should().NotBeNull();
            capturedHistory!.PropertyId.Should().Be(10);
            capturedHistory.OldPrice.Should().Be(0);
            capturedHistory.NewPrice.Should().Be(250000m);
            capturedHistory.Currency.Should().Be(CurrencyConstants.USD);
            capturedHistory.PercentageChange.Should().Be(0);
            capturedHistory.ChangedByUserId.Should().Be("agent-123");
            capturedHistory.ChangeReason.Should().Be("Precio inicial de publicación");
        }

        [Fact]
        public async Task Update_ShouldRecordPriceDropHistory_WhenPriceDecreases()
        {
            // Arrange
            int propertyId = 5;
            var existingProperty = new Property
            {
                Id = propertyId,
                Name = "Apartamento Vista Mar",
                Price = 100000m,
                Currency = CurrencyConstants.USD,
                AgentId = "agent-001"
            };

            var updateVm = new SavePropertyViewModel
            {
                Id = propertyId,
                Name = "Apartamento Vista Mar Renovado",
                Price = 85000m, // Rebaja de $15,000 (-15%)
                Currency = CurrencyConstants.USD,
                AgentId = "agent-001"
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(existingProperty);

            PropertyPriceHistory? capturedHistory = null;
            _priceHistoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()))
                .Callback<PropertyPriceHistory>(h => capturedHistory = h)
                .ReturnsAsync((PropertyPriceHistory h) => h);

            // Act
            await _sut.Update(updateVm, propertyId);

            // Assert
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Property>(p => p.Price == 85000m)), Times.Once);
            _priceHistoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()), Times.Once);

            capturedHistory.Should().NotBeNull();
            capturedHistory!.PropertyId.Should().Be(propertyId);
            capturedHistory.OldPrice.Should().Be(100000m);
            capturedHistory.NewPrice.Should().Be(85000m);
            capturedHistory.PercentageChange.Should().Be(-15.0m);
            capturedHistory.ChangeReason.Should().Contain("Rebaja de precio");
        }

        [Fact]
        public async Task Update_ShouldRecordPriceIncreaseHistory_WhenPriceIncreases()
        {
            // Arrange
            int propertyId = 8;
            var existingProperty = new Property
            {
                Id = propertyId,
                Name = "Penthouse Luxury",
                Price = 200000m,
                Currency = CurrencyConstants.USD,
                AgentId = "agent-002"
            };

            var updateVm = new SavePropertyViewModel
            {
                Id = propertyId,
                Name = "Penthouse Luxury Remodelado",
                Price = 220000m, // Aumento de $20,000 (+10%)
                Currency = CurrencyConstants.USD,
                AgentId = "agent-002"
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(existingProperty);

            PropertyPriceHistory? capturedHistory = null;
            _priceHistoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()))
                .Callback<PropertyPriceHistory>(h => capturedHistory = h)
                .ReturnsAsync((PropertyPriceHistory h) => h);

            // Act
            await _sut.Update(updateVm, propertyId);

            // Assert
            _priceHistoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()), Times.Once);
            capturedHistory.Should().NotBeNull();
            capturedHistory!.OldPrice.Should().Be(200000m);
            capturedHistory.NewPrice.Should().Be(220000m);
            capturedHistory.PercentageChange.Should().Be(10.0m);
            capturedHistory.ChangeReason.Should().Contain("Aumento de precio");
        }

        [Fact]
        public async Task Update_ShouldNotRecordHistory_WhenPriceAndCurrencyRemainUnchanged()
        {
            // Arrange
            int propertyId = 3;
            var existingProperty = new Property
            {
                Id = propertyId,
                Name = "Casa Familiar",
                Price = 150000m,
                Currency = CurrencyConstants.USD,
                AgentId = "agent-003"
            };

            var updateVm = new SavePropertyViewModel
            {
                Id = propertyId,
                Name = "Casa Familiar con Nuevo Jardín", // Solo cambia nombre
                Price = 150000m, // Mismo precio
                Currency = CurrencyConstants.USD, // Misma moneda
                AgentId = "agent-003"
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(existingProperty);

            // Act
            await _sut.Update(updateVm, propertyId);

            // Assert
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Property>()), Times.Once);
            _priceHistoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PropertyPriceHistory>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdViewModel_ShouldCalculatePriceDropMetrics_WhenPropertyHasPriceReduction()
        {
            // Arrange
            int propertyId = 12;
            var property = new Property
            {
                Id = propertyId,
                Name = "Apartamento en Oferta",
                Price = 90000m,
                Currency = CurrencyConstants.USD,
                AgentId = "agent-004"
            };

            var histories = new List<PropertyPriceHistory>
            {
                new() { Id = 1, PropertyId = propertyId, OldPrice = 0, NewPrice = 120000m, Currency = CurrencyConstants.USD, ChangeDate = DateTime.UtcNow.AddMonths(-2) },
                new() { Id = 2, PropertyId = propertyId, OldPrice = 120000m, NewPrice = 90000m, Currency = CurrencyConstants.USD, PercentageChange = -25.0m, ChangeDate = DateTime.UtcNow.AddDays(-5), ChangeReason = "Rebaja de precio" }
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);
            _priceHistoryRepositoryMock.Setup(r => r.GetByPropertyIdAsync(propertyId))
                .ReturnsAsync(histories);

            // Act
            var result = await _sut.GetByIdViewModel(propertyId);

            // Assert
            result.Should().NotBeNull();
            result!.HasPriceDrop.Should().BeTrue();
            result.PriceDropPercentage.Should().Be(25.0m);
            result.PriceDropAmount.Should().Be(30000m);
            result.OriginalPrice.Should().Be(120000m);
            result.PriceHistories.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPriceHistoryAsync_ShouldReturnChronologicalHistory()
        {
            // Arrange
            int propertyId = 7;
            var histories = new List<PropertyPriceHistory>
            {
                new() { Id = 1, PropertyId = propertyId, OldPrice = 0, NewPrice = 100000m, Currency = CurrencyConstants.USD, ChangeDate = DateTime.UtcNow.AddDays(-30) },
                new() { Id = 2, PropertyId = propertyId, OldPrice = 100000m, NewPrice = 95000m, Currency = CurrencyConstants.USD, PercentageChange = -5.0m, ChangeDate = DateTime.UtcNow.AddDays(-10) }
            };

            _priceHistoryRepositoryMock.Setup(r => r.GetByPropertyIdAsync(propertyId))
                .ReturnsAsync(histories);

            // Act
            var result = await _sut.GetPriceHistoryAsync(propertyId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].NewPrice.Should().Be(100000m);
            result[1].NewPrice.Should().Be(95000m);
            result[1].PercentageChange.Should().Be(-5.0m);
        }

        #endregion

        #region Core Property & Code Generation Tests

        [Fact]
        public async Task Add_ShouldGenerateCorrectPrefixForApartment()
        {
            // Arrange
            var vm = new SavePropertyViewModel
            {
                Name = "Apartamento Torre 1",
                Price = 80000m,
                Currency = CurrencyConstants.USD,
                PropertyTypeId = 2,
                AgentId = "agent-1"
            };

            _propertyTypeRepositoryMock.Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(new PropertyType { Id = 2, Name = "Apartamento" });

            _propertyRepositoryMock.Setup(r => r.GetByAgentIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<Property>());

            Property? createdProperty = null;
            _propertyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Property>()))
                .Callback<Property>(p => createdProperty = p)
                .ReturnsAsync((Property p) => { p.Id = 20; return p; });

            // Act
            await _sut.Add(vm);

            // Assert
            createdProperty.Should().NotBeNull();
            createdProperty!.Code.Should().StartWith("APT");
            createdProperty.Status.Should().Be(PropertyStatus.Available);
        }

        [Fact]
        public async Task ToggleFeaturedAsync_ShouldActivateFeatured_WhenPropertyIsNotFeatured()
        {
            // Arrange
            int propertyId = 15;
            var property = new Property
            {
                Id = propertyId,
                Name = "Casa Moderna",
                IsFeatured = false,
                FeaturedUntil = null
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            // Act
            await _sut.ToggleFeaturedAsync(propertyId, durationDays: 15);

            // Assert
            property.IsFeatured.Should().BeTrue();
            property.FeaturedUntil.Should().NotBeNull();
            property.FeaturedUntil.Should().BeAfter(DateTime.UtcNow.AddDays(14));
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(property), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldRemovePropertyAndCleanImageFiles_UsingLoadedImages()
        {
            // Arrange
            int propertyId = 99;
            var property = new Property
            {
                Id = propertyId,
                Images = new List<PropertyImage>
                {
                    new PropertyImage { Id = 1, PropertyId = propertyId, ImageUrl = "img1.png" },
                    new PropertyImage { Id = 2, PropertyId = propertyId, ImageUrl = "img2.png" }
                }
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            // Act
            await _sut.Delete(propertyId);

            // Assert: los archivos se limpian desde las imágenes ya cargadas (sin re-consultar el repositorio de imágenes)
            _fileStorageServiceMock.Verify(f => f.DeleteFileAsync("img1.png", "properties"), Times.Once);
            _fileStorageServiceMock.Verify(f => f.DeleteFileAsync("img2.png", "properties"), Times.Once);
            _propertyImageRepositoryMock.Verify(r => r.GetByPropertyIdAsync(It.IsAny<int>()), Times.Never);
            _propertyRepositoryMock.Verify(r => r.DeleteAsync(property), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldThrowNotFoundException_WhenPropertyDoesNotExist()
        {
            // Arrange
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(1234))
                .ReturnsAsync((Property?)null);

            // Act
            var act = async () => await _sut.Delete(1234);

            // Assert
            await act.Should().ThrowAsync<RealEstateApp.Core.Domain.Exceptions.NotFoundException>();
            _propertyRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Property>()), Times.Never);
            _fileStorageServiceMock.Verify(f => f.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region ToggleFeaturedAsync Tests

        [Fact]
        public async Task ToggleFeaturedAsync_DeberiaLanzarValidationException_CuandoAgenteAlcanzaLimiteDestacados()
        {
            // Arrange
            var propertyId = 10;
            var agentId = "agent-123";
            var property = new Property
            {
                Id = propertyId,
                AgentId = agentId,
                IsFeatured = false,
                FeaturedUntil = null
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            _subscriptionServiceMock.Setup(s => s.CanAgentFeaturePropertyAsync(agentId, propertyId))
                .ReturnsAsync((false, "Has alcanzado el límite de propiedades destacadas"));

            // Act
            var act = async () => await _sut.ToggleFeaturedAsync(propertyId);

            // Assert
            await act.Should().ThrowAsync<RealEstateApp.Core.Domain.Exceptions.ValidationException>()
                .WithMessage("*límite de propiedades destacadas*");
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Property>()), Times.Never);
        }

        [Fact]
        public async Task ToggleFeaturedAsync_DeberiaActivarDestacado_CuandoAgenteTieneCuotaDisponible()
        {
            // Arrange
            var propertyId = 11;
            var agentId = "agent-123";
            var property = new Property
            {
                Id = propertyId,
                AgentId = agentId,
                IsFeatured = false,
                FeaturedUntil = null
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            _subscriptionServiceMock.Setup(s => s.CanAgentFeaturePropertyAsync(agentId, propertyId))
                .ReturnsAsync((true, "Permitido"));

            // Act
            await _sut.ToggleFeaturedAsync(propertyId, 15);

            // Assert
            property.IsFeatured.Should().BeTrue();
            property.FeaturedUntil.Should().NotBeNull();
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(property), Times.Once);
        }

        [Fact]
        public async Task ToggleFeaturedAsync_DeberiaDesactivarDestacado_CuandoInmuebleYaEsDestacado()
        {
            // Arrange
            var propertyId = 12;
            var property = new Property
            {
                Id = propertyId,
                AgentId = "agent-123",
                IsFeatured = true,
                FeaturedUntil = DateTime.UtcNow.AddDays(10)
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(propertyId))
                .ReturnsAsync(property);

            // Act
            await _sut.ToggleFeaturedAsync(propertyId);

            // Assert
            property.IsFeatured.Should().BeFalse();
            property.FeaturedUntil.Should().BeNull();
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(property), Times.Once);
        }

        #endregion
    }
}
