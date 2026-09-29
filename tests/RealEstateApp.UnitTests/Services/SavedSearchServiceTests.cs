using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class SavedSearchServiceTests
    {
        private readonly Mock<ISavedSearchRepository> _savedSearchRepoMock;
        private readonly Mock<IPropertyRepository> _propertyRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly IMapper _mapper;
        private readonly SavedSearchService _sut;

        public SavedSearchServiceTests()
        {
            _savedSearchRepoMock = new Mock<ISavedSearchRepository>();
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _accountServiceMock = new Mock<IAccountService>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _currencyServiceMock.Setup(c => c.GetExchangeRateAsync()).ReturnsAsync(60.0m);

            _sut = new SavedSearchService(
                _savedSearchRepoMock.Object,
                _propertyRepoMock.Object,
                _emailServiceMock.Object,
                _accountServiceMock.Object,
                _mapper,
                _currencyServiceMock.Object);
        }

        [Fact]
        public async Task CheckAndNotifyMatchesAsync_DebeEnviarCorreo_CuandoHayCoincidenciaConAlertasHabilitadas()
        {
            // Arrange
            var property = new Property
            {
                Id = 10,
                Name = "Apartamento de Lujo",
                Price = 3000000m,
                Currency = CurrencyConstants.DOP,
                Rooms = 3,
                Bathrooms = 2,
                SizeInMeters = 120,
                Sector = "Piantini"
            };

            var matchingSearches = new List<SavedSearch>
            {
                new SavedSearch
                {
                    Id = 1,
                    UserId = "user-100",
                    Name = "Apartamentos en Piantini",
                    EmailAlertsEnabled = true
                }
            };

            var userDto = new AccountUserDto
            {
                Id = "user-100",
                UserName = "juanperez",
                Email = "juan@example.com",
                FirstName = "Juan",
                LastName = "Perez"
            };

            _savedSearchRepoMock.Setup(r => r.GetMatchingSearchesAsync(property))
                .ReturnsAsync(matchingSearches);

            _accountServiceMock.Setup(a => a.GetUserByIdAsync("user-100"))
                .ReturnsAsync(userDto);

            _emailServiceMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.CheckAndNotifyMatchesAsync(property);

            // Assert
            _emailServiceMock.Verify(e => e.SendAsync(
                "juan@example.com",
                It.Is<string>(s => s.Contains("Apartamentos en Piantini")),
                It.Is<string>(b => b.Contains("Juan Perez") && b.Contains("Apartamento de Lujo"))
            ), Times.Once);

            _savedSearchRepoMock.Verify(r => r.UpdateAsync(It.Is<SavedSearch>(s => s.Id == 1 && s.LastAlertSent != null)), Times.Once);
        }

        [Fact]
        public async Task CheckAndNotifyMatchesAsync_NoDebeEnviarCorreo_CuandoEmailAlertsEstaDeshabilitado()
        {
            // Arrange
            var property = new Property { Id = 10, Name = "Casa", Price = 2000000m, Currency = "DOP" };
            var matchingSearches = new List<SavedSearch>
            {
                new SavedSearch
                {
                    Id = 2,
                    UserId = "user-200",
                    Name = "Búsqueda Silenciosa",
                    EmailAlertsEnabled = false
                }
            };

            _savedSearchRepoMock.Setup(r => r.GetMatchingSearchesAsync(property))
                .ReturnsAsync(matchingSearches);

            // Act
            await _sut.CheckAndNotifyMatchesAsync(property);

            // Assert
            _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
