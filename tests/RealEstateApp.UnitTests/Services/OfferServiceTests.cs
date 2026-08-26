using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class OfferServiceTests
    {
        private readonly Mock<IOfferRepository> _offerRepositoryMock;
        private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
        private readonly Mock<IUserActivityService> _userActivityServiceMock;
        private readonly IMapper _mapper;
        private readonly OfferService _sut;

        public OfferServiceTests()
        {
            _offerRepositoryMock = new Mock<IOfferRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _userActivityServiceMock = new Mock<IUserActivityService>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _sut = new OfferService(
                _offerRepositoryMock.Object,
                _propertyRepositoryMock.Object,
                _userActivityServiceMock.Object,
                _mapper
            );
        }

        [Fact]
        public async Task Add_ShouldCreatePendingOffer_WhenPropertyIsAvailable()
        {
            // Arrange
            var property = new Property
            {
                Id = 1,
                Status = PropertyStatus.Available,
                Price = 100000m
            };

            var saveVm = new SaveOfferViewModel
            {
                PropertyId = 1,
                MontoOfertado = 95000m
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(property);

            Offer? capturedOffer = null;
            _offerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Offer>()))
                .Callback<Offer>(o => capturedOffer = o)
                .ReturnsAsync((Offer o) => o);

            // Act
            var result = await _sut.Add(saveVm, "client-001");

            // Assert
            result.Should().NotBeNull();
            _offerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Offer>()), Times.Once);
            capturedOffer.Should().NotBeNull();
            capturedOffer!.Status.Should().Be(OfferStatus.Pending);
            capturedOffer.ClienteId.Should().Be("client-001");
            capturedOffer.MontoOfertado.Should().Be(95000m);
        }

        [Fact]
        public async Task Add_ShouldThrowValidationException_WhenPropertyIsNotAvailable()
        {
            // Arrange
            var property = new Property
            {
                Id = 2,
                Status = PropertyStatus.Sold,
                Price = 100000m
            };

            var saveVm = new SaveOfferViewModel
            {
                PropertyId = 2,
                MontoOfertado = 90000m
            };

            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(property);

            // Act
            Func<Task> act = async () => await _sut.Add(saveVm, "client-001");

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("La propiedad no está disponible para ofertas");
        }

        [Fact]
        public async Task CounterOffer_ShouldUpdateStatusAndLogActivity()
        {
            // Arrange
            int offerId = 10;
            var offer = new Offer
            {
                Id = offerId,
                Status = OfferStatus.Pending,
                MontoOfertado = 80000m,
                ClienteId = "client-001"
            };

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId))
                .ReturnsAsync(offer);

            // Act
            await _sut.CounterOffer(offerId, 85000m, "Monto mínimo aceptable", "agent-001");

            // Assert
            offer.Status.Should().Be(OfferStatus.CounterOffered);
            offer.CounterOfferAmount.Should().Be(85000m);
            offer.CounterOfferMessage.Should().Be("Monto mínimo aceptable");
            _offerRepositoryMock.Verify(r => r.UpdateAsync(offer), Times.Once);
            _userActivityServiceMock.Verify(u => u.LogActivityAsync(
                "agent-001",
                "Contra-Oferta Enviada",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task RejectOffer_ShouldSetStatusToRejected()
        {
            // Arrange
            int offerId = 14;
            var offer = new Offer
            {
                Id = offerId,
                Status = OfferStatus.Pending
            };

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId))
                .ReturnsAsync(offer);

            // Act
            await _sut.RejectOffer(offerId);

            // Assert
            offer.Status.Should().Be(OfferStatus.Rejected);
            _offerRepositoryMock.Verify(r => r.UpdateAsync(offer), Times.Once);
        }
    }
}
