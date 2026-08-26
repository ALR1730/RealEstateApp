using System;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Constants;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class CurrencyServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<CurrencyService>> _loggerMock;
        private readonly CurrencyService _sut;

        public CurrencyServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<CurrencyService>>();

            _sut = new CurrencyService(
                _httpClientFactoryMock.Object,
                _memoryCache,
                _loggerMock.Object
            );
        }

        [Fact]
        public void ConvertToDOP_ShouldMultiplyByExchangeRate_WhenCurrencyIsUSD()
        {
            // Arrange
            decimal amountUsd = 1000m;
            decimal exchangeRate = 60.0m;

            // Act
            decimal result = _sut.ConvertToDOP(amountUsd, CurrencyConstants.USD, exchangeRate);

            // Assert
            result.Should().Be(60000m);
        }

        [Fact]
        public void ConvertToDOP_ShouldReturnSameAmount_WhenCurrencyIsAlreadyDOP()
        {
            // Arrange
            decimal amountDop = 50000m;
            decimal exchangeRate = 60.0m;

            // Act
            decimal result = _sut.ConvertToDOP(amountDop, CurrencyConstants.DOP, exchangeRate);

            // Assert
            result.Should().Be(50000m);
        }

        [Fact]
        public void ConvertToUSD_ShouldDivideByExchangeRate_WhenCurrencyIsDOP()
        {
            // Arrange
            decimal amountDop = 120000m;
            decimal exchangeRate = 60.0m;

            // Act
            decimal result = _sut.ConvertToUSD(amountDop, CurrencyConstants.DOP, exchangeRate);

            // Assert
            result.Should().Be(2000m);
        }

        [Fact]
        public void FormatPrice_ShouldFormatUSDProperly()
        {
            // Arrange
            decimal amount = 150000m;

            // Act
            string formatted = _sut.FormatPrice(amount, CurrencyConstants.USD);

            // Assert
            formatted.Should().Be("US$ 150,000");
        }

        [Fact]
        public void FormatPrice_ShouldFormatDOPProperly()
        {
            // Arrange
            decimal amount = 8500000m;

            // Act
            string formatted = _sut.FormatPrice(amount, CurrencyConstants.DOP);

            // Assert
            formatted.Should().Be("RD$ 8,500,000");
        }

        [Fact]
        public void FormatSecondaryPrice_ShouldFormatOppositeCurrencyWithTilde()
        {
            // Arrange
            decimal amountUsd = 1000m;
            decimal rate = 60.0m;

            // Act: active currency is USD, secondary should be DOP (60,000)
            string result = _sut.FormatSecondaryPrice(amountUsd, CurrencyConstants.USD, CurrencyConstants.USD, rate);

            // Assert
            result.Should().Be("~RD$ 60,000");
        }
    }
}
