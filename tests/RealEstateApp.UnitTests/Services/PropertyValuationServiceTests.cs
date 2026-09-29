using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class PropertyValuationServiceTests
    {
        private readonly Mock<IPropertyRepository> _propertyRepoMock;
        private readonly Mock<IPropertyValuationRepository> _valuationRepoMock;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly IMapper _mapper;
        private readonly PropertyValuationService _sut;

        public PropertyValuationServiceTests()
        {
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _valuationRepoMock = new Mock<IPropertyValuationRepository>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _currencyServiceMock.Setup(c => c.GetExchangeRateAsync()).ReturnsAsync(60.0m);
            _currencyServiceMock.Setup(c => c.ConvertToDOP(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns<decimal, string, decimal>((price, curr, rate) => curr == "USD" ? price * rate : price);

            _sut = new PropertyValuationService(
                _propertyRepoMock.Object,
                _valuationRepoMock.Object,
                _currencyServiceMock.Object,
                _mapper);
        }

        [Fact]
        public async Task CalculateValuationAsync_DebeCalcularValuacion_CuandoExistenComparablesCercanos()
        {
            // Arrange
            var targetProperty = new Property
            {
                Id = 1,
                Name = "Apartamento Target",
                Code = "APT001",
                Price = 5000000m,
                Currency = CurrencyConstants.DOP,
                SizeInMeters = 100,
                MunicipalityId = 10,
                ProvinceId = 1,
                Sector = "Bella Vista",
                Latitude = 18.4500,
                Longitude = -69.9500,
                Status = PropertyStatus.Available
            };

            var comparables = new List<Property>
            {
                new Property
                {
                    Id = 2,
                    Name = "Apartamento Comp 1",
                    Code = "APT002",
                    Price = 5200000m,
                    PriceInDOP = 5200000m,
                    Currency = CurrencyConstants.DOP,
                    SizeInMeters = 100,
                    MunicipalityId = 10,
                    ProvinceId = 1,
                    Sector = "Bella Vista",
                    Latitude = 18.4510,
                    Longitude = -69.9510,
                    Status = PropertyStatus.Available
                },
                new Property
                {
                    Id = 3,
                    Name = "Apartamento Comp 2",
                    Code = "APT003",
                    Price = 4800000m,
                    PriceInDOP = 4800000m,
                    Currency = CurrencyConstants.DOP,
                    SizeInMeters = 100,
                    MunicipalityId = 10,
                    ProvinceId = 1,
                    Sector = "Bella Vista",
                    Latitude = 18.4520,
                    Longitude = -69.9520,
                    Status = PropertyStatus.Available
                }
            };

            _propertyRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(targetProperty);

            _propertyRepoMock.Setup(r => r.GetCandidateComparablesAsync(1, 10, 1))
                .ReturnsAsync(comparables);

            _valuationRepoMock.Setup(r => r.AddAsync(It.IsAny<PropertyValuation>()))
                .ReturnsAsync((PropertyValuation v) => { v.Id = 1; return v; });

            // Act
            var result = await _sut.CalculateValuationAsync(1, searchRadiusKm: 5.0m);

            // Assert
            result.Should().NotBeNull();
            result.PropertyId.Should().Be(1);
            result.ComparableCount.Should().Be(2);
            result.EstimatedPricePerSqm.Should().Be(50000m);
            result.EstimatedTotalPrice.Should().Be(5000000m);
            result.ValuationRating.Should().Be("Justa");
            _valuationRepoMock.Verify(r => r.AddAsync(It.IsAny<PropertyValuation>()), Times.Once);
        }

        [Fact]
        public async Task CalculateValuationAsync_DebeLanzarNotFoundException_CuandoPropiedadNoExiste()
        {
            // Arrange
            _propertyRepoMock.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Property?)null);

            // Act
            Func<Task> act = async () => await _sut.CalculateValuationAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
