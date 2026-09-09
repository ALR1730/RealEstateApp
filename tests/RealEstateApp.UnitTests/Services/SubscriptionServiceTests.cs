using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Subscription;
using RealEstateApp.Core.Domain.Entities;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionPlanRepository> _planRepositoryMock;
        private readonly Mock<IAgentSubscriptionRepository> _agentSubscriptionRepositoryMock;
        private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
        private readonly SubscriptionService _sut;

        public SubscriptionServiceTests()
        {
            _planRepositoryMock = new Mock<ISubscriptionPlanRepository>();
            _agentSubscriptionRepositoryMock = new Mock<IAgentSubscriptionRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();

            _sut = new SubscriptionService(
                _planRepositoryMock.Object,
                _agentSubscriptionRepositoryMock.Object,
                _propertyRepositoryMock.Object);
        }

        private static SaveSubscriptionPlanViewModel BuildValidModel() => new SaveSubscriptionPlanViewModel
        {
            Name = "Plan Ejecutivo",
            Description = "Plan para firmas inmobiliarias.",
            MonthlyPrice = 99.99m,
            MaxActiveProperties = 25,
            MaxFeaturedProperties = 5,
            Allows3DTours = true,
            AllowsVideo = false,
            CommissionPercentage = 4.25m,
            IsActive = true
        };

        [Fact]
        public async Task GetAllPlansAsync_DevuelveTodosLosPlanesOrdenadosPorPrecio()
        {
            // Arrange
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan { Id = 1, Name = "Premium", MonthlyPrice = 129.99m },
                new SubscriptionPlan { Id = 2, Name = "Gratuito", MonthlyPrice = 0m },
                new SubscriptionPlan { Id = 3, Name = "Pro", MonthlyPrice = 49.99m, IsActive = false }
            };
            _planRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(plans);

            // Act
            var result = await _sut.GetAllPlansAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Select(p => p.MonthlyPrice).Should().BeInAscendingOrder();
            result.Should().ContainSingle(p => p.Id == 3 && p.IsActive == false);
        }

        [Fact]
        public async Task GetPlanByIdAsync_PlanExistente_DevuelveViewModel()
        {
            // Arrange
            var plan = new SubscriptionPlan { Id = 7, Name = "Pro", MonthlyPrice = 49.99m };
            _planRepositoryMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(plan);

            // Act
            var result = await _sut.GetPlanByIdAsync(7);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(7);
            result.Name.Should().Be("Pro");
            result.MonthlyPrice.Should().Be(49.99m);
        }

        [Fact]
        public async Task GetPlanByIdAsync_PlanInexistente_DevuelveNull()
        {
            // Arrange
            _planRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _sut.GetPlanByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreatePlanAsync_MapeaElNuevoPlanYLoPersiste()
        {
            // Arrange
            var model = BuildValidModel();
            SubscriptionPlan? saved = null;
            _planRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SubscriptionPlan>()))
                .Callback<SubscriptionPlan>(p => saved = p)
                .ReturnsAsync((SubscriptionPlan p) => p);

            // Act
            var result = await _sut.CreatePlanAsync(model);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Plan Ejecutivo");
            result.MaxActiveProperties.Should().Be(25);
            result.MaxFeaturedProperties.Should().Be(5);
            result.CommissionPercentage.Should().Be(4.25m);
            result.AllowsVideo.Should().BeFalse();
            _planRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SubscriptionPlan>()), Times.Once);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Plan Ejecutivo");
        }

        [Fact]
        public async Task CreatePlanAsync_NombreVacio_DevuelveNull()
        {
            // Arrange
            var model = BuildValidModel();
            model.Name = "   ";

            // Act
            var result = await _sut.CreatePlanAsync(model);

            // Assert
            result.Should().BeNull();
            _planRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePlanAsync_ActualizaLosCamposDelPlan()
        {
            // Arrange
            var plan = new SubscriptionPlan
            {
                Id = 1,
                Name = "Gratuito",
                MonthlyPrice = 0m,
                MaxActiveProperties = 3,
                CommissionPercentage = 5m
            };
            _planRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
            var model = BuildValidModel();

            // Act
            var result = await _sut.UpdatePlanAsync(1, model);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Plan Ejecutivo");
            result.MonthlyPrice.Should().Be(99.99m);
            plan.MaxFeaturedProperties.Should().Be(5);
            _planRepositoryMock.Verify(r => r.UpdateAsync(plan), Times.Once);
        }

        [Fact]
        public async Task UpdatePlanAsync_PlanInexistente_DevuelveNull()
        {
            // Arrange
            _planRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _sut.UpdatePlanAsync(99, BuildValidModel());

            // Assert
            result.Should().BeNull();
            _planRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
        }

        [Fact]
        public async Task SetPlanActiveAsync_ActivaODesactivaElPlan()
        {
            // Arrange
            var plan = new SubscriptionPlan { Id = 2, Name = "Pro", IsActive = true };
            _planRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(plan);

            // Act
            var result = await _sut.SetPlanActiveAsync(2, false);

            // Assert
            result.Should().BeTrue();
            plan.IsActive.Should().BeFalse();
            _planRepositoryMock.Verify(r => r.UpdateAsync(plan), Times.Once);
        }

        [Fact]
        public async Task SetPlanActiveAsync_PlanInexistente_DevuelveFalse()
        {
            // Arrange
            _planRepositoryMock.Setup(r => r.GetByIdAsync(55)).ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _sut.SetPlanActiveAsync(55, true);

            // Assert
            result.Should().BeFalse();
            _planRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
        }

        [Fact]
        public async Task DeletePlanAsync_PlanSinAgentesSuscritos_EliminaYDevuelveTrue()
        {
            // Arrange
            var plan = new SubscriptionPlan { Id = 4, Name = "Starter" };
            _planRepositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(plan);
            _agentSubscriptionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AgentSubscription>());

            // Act
            var result = await _sut.DeletePlanAsync(4);

            // Assert
            result.Should().BeTrue();
            _planRepositoryMock.Verify(r => r.DeleteAsync(plan), Times.Once);
        }

        [Fact]
        public async Task DeletePlanAsync_PlanConAgentesSuscritos_NoEliminaYDevuelveFalse()
        {
            // Arrange
            var plan = new SubscriptionPlan { Id = 4, Name = "Starter" };
            _planRepositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(plan);
            _agentSubscriptionRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AgentSubscription>
            {
                new AgentSubscription { Id = 1, AgentId = "agent-1", SubscriptionPlanId = 4 }
            });

            // Act
            var result = await _sut.DeletePlanAsync(4);

            // Assert
            result.Should().BeFalse();
            _planRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
        }

        [Fact]
        public async Task DeletePlanAsync_PlanInexistente_DevuelveFalse()
        {
            // Arrange
            _planRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SubscriptionPlan?)null);

            // Act
            var result = await _sut.DeletePlanAsync(99);

            // Assert
            result.Should().BeFalse();
            _planRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
        }
    }
}