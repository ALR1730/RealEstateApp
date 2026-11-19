using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces;
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
        private readonly Mock<ICommissionService> _commissionServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IMapper _mapper;
        private readonly OfferService _sut;

        public OfferServiceTests()
        {
            _offerRepositoryMock = new Mock<IOfferRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _userActivityServiceMock = new Mock<IUserActivityService>();
            _commissionServiceMock = new Mock<ICommissionService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _unitOfWorkMock
                .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default))
                .Returns<Func<Task>, CancellationToken>(async (action, _) => await action());

            _offerRepositoryMock
                .Setup(r => r.GetActiveCompetingOffersAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Offer>());

            _sut = new OfferService(
                _offerRepositoryMock.Object,
                _propertyRepositoryMock.Object,
                _userActivityServiceMock.Object,
                _commissionServiceMock.Object,
                _mapper,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Add_ShouldCreatePendingOffer_WhenPropertyIsAvailable()
        {
            // Arrange
            var property = new Property
            {
                Id = 1,
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
                Price = 100000m
            };
            property.MarkAsSold();

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
                PropertyId = 1,
                MontoOfertado = 80000m,
                ClienteId = "client-001"
            };

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId))
                .ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.GetAgentIdByPropertyIdAsync(1))
                .ReturnsAsync("agent-001");

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
                PropertyId = 1
            };

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId))
                .ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.GetAgentIdByPropertyIdAsync(1))
                .ReturnsAsync("agent-001");

            // Act
            await _sut.RejectOffer(offerId, "agent-001");

            // Assert
            offer.Status.Should().Be(OfferStatus.Rejected);
            _offerRepositoryMock.Verify(r => r.UpdateAsync(offer), Times.Once);
        }

        [Fact]
        public async Task AcceptCounterOffer_DebeAceptarContraofertaYVenderPropiedadAtomicamente_CuandoClienteEsPropietario()
        {
            // Arrange
            int offerId = 15;
            var property = new Property { Id = 1, AgentId = "agent-001" };
            var offer = new Offer
            {
                Id = offerId,
                PropertyId = 1,
                ClienteId = "client-001",
                MontoOfertado = 82000m
            };
            offer.MakeCounterOffer(87000m, null);

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId))
                .ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(property);

            // Act
            await _sut.AcceptCounterOffer(offerId, "client-001");

            // Assert
            offer.Status.Should().Be(OfferStatus.Accepted);
            property.Status.Should().Be(PropertyStatus.Sold);
            _unitOfWorkMock.Verify(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default), Times.Once);
            _offerRepositoryMock.Verify(r => r.UpdateAsync(offer), Times.Once);
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(property), Times.Once);
            _offerRepositoryMock.Verify(r => r.GetActiveCompetingOffersAsync(1, offerId), Times.Once);
            _commissionServiceMock.Verify(c => c.CreateForAcceptedOfferAsync(offerId), Times.Once);
            _userActivityServiceMock.Verify(u => u.LogActivityAsync(
                "client-001",
                "Contra-Oferta Aceptada",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }

        [Fact]
        public async Task AcceptCounterOffer_ShouldThrow_WhenClientIsNotOwner()
        {
            // Arrange
            int offerId = 16;
            var offer = new Offer
            {
                Id = offerId,
                ClienteId = "client-001",
                MontoOfertado = 80000m
            };
            offer.MakeCounterOffer(85000m, null);

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId))
                .ReturnsAsync(offer);

            // Act
            var act = async () => await _sut.AcceptCounterOffer(offerId, "client-002");

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("No tiene permisos para modificar esta oferta");
            _offerRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Offer>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default), Times.Never);
        }

        [Fact]
        public async Task AcceptOffer_DebeEjecutarTransaccionAtomica_CuandoUnitOfWorkEstaConfigurado()
        {
            // Arrange
            int offerId = 42;
            var property = new Property { Id = 1, AgentId = "agent-001" };
            var offer = new Offer
            {
                Id = offerId,
                PropertyId = 1,
                MontoOfertado = 4_000_000m
            };
            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId)).ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.GetAgentIdByPropertyIdAsync(1))
                .ReturnsAsync("agent-001");
            _propertyRepositoryMock.Setup(r => r.FindByIdAsync(1))
                .ReturnsAsync(property);

            // Act
            await _sut.AcceptOffer(offerId, "agent-001");

            // Assert
            offer.Status.Should().Be(OfferStatus.Accepted);
            property.Status.Should().Be(PropertyStatus.Sold);
            _unitOfWorkMock.Verify(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default), Times.Once);
            _offerRepositoryMock.Verify(r => r.UpdateAsync(offer), Times.Once);
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(property), Times.Once);
            _commissionServiceMock.Verify(c => c.CreateForAcceptedOfferAsync(offerId), Times.Once);
        }

        /// <summary>
        /// Tarea 1.5 / Hallazgo 1.3: AcceptCounterOffer debe envolver las operaciones críticas
        /// en una transacción UoW para garantizar atomicidad ACID. Si la comisión falla,
        /// el estado de la oferta y la propiedad no debe quedar comprometido.
        /// </summary>
        [Fact]
        public async Task AcceptCounterOffer_DebeEjecutarseEnTransaccionUoW_CuandoSeAceptaContraoferta()
        {
            // Arrange
            int offerId = 55;
            var property = new Property { Id = 2, AgentId = "agent-001" };
            var offer = new Offer
            {
                Id = offerId,
                PropertyId = 2,
                ClienteId = "client-001",
                MontoOfertado = 5_000_000m
            };
            offer.MakeCounterOffer(5_500_000m, null);

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(offerId)).ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.FindByIdAsync(2)).ReturnsAsync(property);

            // Act
            await _sut.AcceptCounterOffer(offerId, "client-001");

            // Assert: la transacción UoW debe envolver la aceptación, el cierre de propiedad/competidoras y la comisión
            _unitOfWorkMock.Verify(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default), Times.Once,
                "Las operaciones de cierre deben ejecutarse dentro de una transacción UoW para garantizar atomicidad ACID");
            offer.Status.Should().Be(OfferStatus.Accepted);
            property.Status.Should().Be(PropertyStatus.Sold);
            _offerRepositoryMock.Verify(r => r.UpdateAsync(offer), Times.Once);
            _propertyRepositoryMock.Verify(r => r.UpdateAsync(property), Times.Once);
            _commissionServiceMock.Verify(c => c.CreateForAcceptedOfferAsync(offerId), Times.Once);
            _userActivityServiceMock.Verify(u => u.LogActivityAsync(
                "client-001",
                "Contra-Oferta Aceptada",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
        }
    }
}
