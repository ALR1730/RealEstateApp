using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Commission;
using RealEstateApp.Core.Application.ViewModels.Subscription;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class CommissionServiceTests
    {
        private readonly Mock<ICommissionRepository> _commissionRepositoryMock;
        private readonly Mock<IOfferRepository> _offerRepositoryMock;
        private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly CommissionService _sut;

        public CommissionServiceTests()
        {
            _commissionRepositoryMock = new Mock<ICommissionRepository>();
            _offerRepositoryMock = new Mock<IOfferRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            var store = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _sut = new CommissionService(
                _commissionRepositoryMock.Object,
                _offerRepositoryMock.Object,
                _propertyRepositoryMock.Object,
                _subscriptionServiceMock.Object,
                _userManagerMock.Object);
        }

        [Fact]
        public async Task CreateForAcceptedOfferAsync_CalculaMontoConTasaDelPlan()
        {
            // Arrange
            var offer = new Offer { Id = 1, PropertyId = 5, MontoOfertado = 200000m, Status = OfferStatus.Accepted };
            var property = new Property { Id = 5, AgentId = "agent-1", Name = "Casa", Code = "CSA001" };

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);
            _commissionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Commission>());
            _subscriptionServiceMock.Setup(s => s.GetCurrentSubscriptionByAgentIdAsync("agent-1"))
                .ReturnsAsync(new AgentSubscriptionViewModel { CommissionPercentage = 4.50m });

            // Act
            await _sut.CreateForAcceptedOfferAsync(1);

            // Assert
            _commissionRepositoryMock.Verify(r => r.AddAsync(It.Is<Commission>(c =>
                c.AgentId == "agent-1" &&
                c.PropertyId == 5 &&
                c.OfferId == 1 &&
                c.SalePrice == 200000m &&
                c.Rate == 4.50m &&
                c.Amount == 9000m &&
                c.Status == CommissionStatus.Pending)), Times.Once);
        }

        [Fact]
        public async Task CreateForAcceptedOfferAsync_SinTasaEnPlan_UsaDefaultGlobal()
        {
            // Arrange
            var offer = new Offer { Id = 2, PropertyId = 6, MontoOfertado = 100000m };
            var property = new Property { Id = 6, AgentId = "agent-2", Name = "Apto", Code = "APT001" };

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(property);
            _commissionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Commission>());
            _subscriptionServiceMock.Setup(s => s.GetCurrentSubscriptionByAgentIdAsync("agent-2"))
                .ReturnsAsync(new AgentSubscriptionViewModel { CommissionPercentage = null });

            // Act
            await _sut.CreateForAcceptedOfferAsync(2);

            // Assert
            _commissionRepositoryMock.Verify(r => r.AddAsync(It.Is<Commission>(c =>
                c.Rate == CommissionConstants.DefaultRate &&
                c.Amount == 5000m)), Times.Once);
        }

        [Fact]
        public async Task CreateForAcceptedOfferAsync_YaExisteComisionParaOferta_NoDuplica()
        {
            // Arrange
            var offer = new Offer { Id = 3, PropertyId = 7, MontoOfertado = 100000m };
            var property = new Property { Id = 7, AgentId = "agent-1", Name = "Solo", Code = "SOL001" };

            _offerRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(offer);
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(property);
            _commissionRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Commission> { new Commission { OfferId = 3, AgentId = "agent-1" } });

            // Act
            await _sut.CreateForAcceptedOfferAsync(3);

            // Assert
            _commissionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Commission>()), Times.Never);
        }

        [Fact]
        public async Task GetAgentSummaryAsync_CalculaTotalesPendientesYPagadas()
        {
            // Arrange
            var commissions = new List<Commission>
            {
                new Commission { AgentId = "agent-1", Amount = 5000m, Rate = 5m, Status = CommissionStatus.Pending },
                new Commission { AgentId = "agent-1", Amount = 3000m, Rate = 5m, Status = CommissionStatus.Paid },
                new Commission { AgentId = "agent-1", Amount = 7000m, Rate = 4m, Status = CommissionStatus.Paid }
            };
            _commissionRepositoryMock.Setup(r => r.GetByAgentIdAsync("agent-1")).ReturnsAsync(commissions);

            // Act
            var summary = await _sut.GetAgentSummaryAsync("agent-1");

            // Assert
            summary.TotalCount.Should().Be(3);
            summary.TotalAmount.Should().Be(15000m);
            summary.PendingCount.Should().Be(1);
            summary.PendingAmount.Should().Be(5000m);
            summary.PaidCount.Should().Be(2);
            summary.PaidAmount.Should().Be(10000m);
            summary.AverageRate.Should().Be(4.67m);
        }

        [Fact]
        public async Task MarkAsPaidAsync_CambiaEstadoAPagada()
        {
            // Arrange
            var commission = new Commission { Id = 9, Status = CommissionStatus.Pending };
            _commissionRepositoryMock.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(commission);

            // Act
            await _sut.MarkAsPaidAsync(9);

            // Assert
            commission.Status.Should().Be(CommissionStatus.Paid);
            _commissionRepositoryMock.Verify(r => r.UpdateAsync(commission), Times.Once);
        }

        [Fact]
        public async Task MarkAsPaidAsync_ComisionInexistente_ThrowsNotFoundException()
        {
            // Arrange
            _commissionRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Commission?)null);

            // Act
            Func<Task> act = async () => await _sut.MarkAsPaidAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetAllAsync_ResuelvePropiedadYNombreDeAgente()
        {
            // Arrange
            var commissions = new List<Commission>
            {
                new Commission
                {
                    Id = 1,
                    AgentId = "agent-1",
                    PropertyId = 5,
                    OfferId = 3,
                    SalePrice = 200000m,
                    Rate = 5m,
                    Amount = 10000m,
                    Status = CommissionStatus.Pending,
                    Property = new Property { Id = 5, Code = "APT001", Name = "Apto Centro" },
                    Offer = new Offer { Id = 3 }
                }
            };
            _commissionRepositoryMock.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(commissions);

            var agent = new IdentityUser { Id = "agent-1", UserName = "agentuser", Email = "agent@test.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync("agent-1")).ReturnsAsync(agent);
            _userManagerMock.Setup(u => u.GetClaimsAsync(agent)).ReturnsAsync(new List<Claim>
            {
                new("FirstName", "Carlos"),
                new("LastName", "Mendoza")
            });

            // Act
            var result = await _sut.GetAllAsync();

            // Assert
            var vm = result.Should().ContainSingle().Subject;
            vm.PropertyCode.Should().Be("APT001");
            vm.PropertyName.Should().Be("Apto Centro");
            vm.AgentName.Should().Be("Carlos Mendoza");
            vm.Amount.Should().Be(10000m);
            _commissionRepositoryMock.Verify(r => r.GetAllWithDetailsAsync(), Times.Once);
        }
    }
}