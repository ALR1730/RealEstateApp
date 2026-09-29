using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class PropertyEnrichmentServiceTests
    {
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IPropertyPriceHistoryRepository> _priceHistoryRepoMock;
        private readonly PropertyEnrichmentService _sut;

        public PropertyEnrichmentServiceTests()
        {
            _currencyServiceMock = new Mock<ICurrencyService>();
            _accountServiceMock = new Mock<IAccountService>();
            _priceHistoryRepoMock = new Mock<IPropertyPriceHistoryRepository>();

            _currencyServiceMock.Setup(c => c.GetExchangeRateAsync()).ReturnsAsync(60.0m);
            _currencyServiceMock.Setup(c => c.FormatPrice(It.IsAny<decimal>(), It.IsAny<string>()))
                .Returns<decimal, string>((p, c) => $"{c} {p:N2}");
            _currencyServiceMock.Setup(c => c.FormatSecondaryPrice(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns("US$ 1,000.00");

            _sut = new PropertyEnrichmentService(
                _currencyServiceMock.Object,
                _accountServiceMock.Object,
                _priceHistoryRepoMock.Object);
        }

        [Fact]
        public async Task EnrichPropertiesWithCurrencyAsync_DebeCalcularPreciosEnDOPYUSD()
        {
            // Arrange
            var viewModels = new List<PropertyViewModel>
            {
                new PropertyViewModel { Id = 1, Price = 100000m, Currency = CurrencyConstants.USD }
            };

            _priceHistoryRepoMock.Setup(r => r.GetLatestByPropertyIdAsync(1))
                .ReturnsAsync((PropertyPriceHistory?)null);

            // Act
            await _sut.EnrichPropertiesWithCurrencyAsync(viewModels);

            // Assert
            viewModels[0].PriceInUSD.Should().Be(100000m);
            viewModels[0].PriceInDOP.Should().Be(6000000m); // 100,000 * 60
            viewModels[0].DisplayPrice.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task EnrichPriceDropAsync_DebeDetectarRebaja_CuandoUltimoPrecioEsMenor()
        {
            // Arrange
            var vm = new PropertyViewModel { Id = 5, Price = 90000m };
            var history = new PropertyPriceHistory
            {
                PropertyId = 5,
                OldPrice = 100000m,
                NewPrice = 90000m,
                PercentageChange = -10.0m
            };

            _priceHistoryRepoMock.Setup(r => r.GetLatestByPropertyIdAsync(5))
                .ReturnsAsync(history);

            // Act
            await _sut.EnrichPriceDropAsync(vm);

            // Assert
            vm.HasPriceDrop.Should().BeTrue();
            vm.PriceDropPercentage.Should().Be(10.0m);
            vm.PriceDropAmount.Should().Be(10000m);
        }

        [Fact]
        public async Task EnrichAgentNamesAsync_DebePoblarNombreCompletoDelAgente()
        {
            // Arrange
            var viewModels = new List<PropertyViewModel>
            {
                new PropertyViewModel { Id = 1, AgentId = "agent-42" }
            };

            var userDict = new Dictionary<string, AccountUserDto>
            {
                ["agent-42"] = new AccountUserDto { Id = "agent-42", FirstName = "Maria", LastName = "Santos" }
            };

            _accountServiceMock.Setup(a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(userDict);

            // Act
            await _sut.EnrichAgentNamesAsync(viewModels);

            // Assert
            viewModels[0].AgentName.Should().Be("Maria Santos");
        }
    }
}
