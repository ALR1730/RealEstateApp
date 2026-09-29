using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.DTOs.Dashboard;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAgentService> _agentServiceMock;
        private readonly Mock<IPropertyService> _propertyServiceMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AdminController _sut;

        public AdminControllerTests()
        {
            _agentServiceMock = new Mock<IAgentService>();
            _propertyServiceMock = new Mock<IPropertyService>();
            _accountServiceMock = new Mock<IAccountService>();

            var store = new Mock<IUserStore<IdentityUser>>();
            _userManager = new UserManager<IdentityUser>(
                store.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            _sut = new AdminController(
                _agentServiceMock.Object,
                _propertyServiceMock.Object,
                _accountServiceMock.Object,
                _userManager);
        }

        [Fact]
        public async Task GetDashboardKPIsAsync_DebeRetornarMetricasAgregadas_ConStatus200Ok()
        {
            // Arrange
            var propertyMetrics = new PropertyDashboardMetricsDto
            {
                TotalAvailableProperties = 12,
                TotalReservedProperties = 4,
                TotalSoldProperties = 8,
                TotalProperties = 24,
                PropertiesByType = new List<PropertyTypeCountDto>
                {
                    new() { TypeName = "Apartamento", Count = 14 },
                    new() { TypeName = "Casa", Count = 10 }
                }
            };

            var userMetrics = new UserDashboardMetricsDto
            {
                TotalActiveAgents = 5,
                TotalInactiveAgents = 2,
                TotalClients = 30,
                TotalDevelopers = 3
            };

            _propertyServiceMock.Setup(s => s.GetDashboardMetricsAsync())
                .ReturnsAsync(propertyMetrics);

            _accountServiceMock.Setup(s => s.GetUserDashboardMetricsAsync())
                .ReturnsAsync(userMetrics);

            // Act
            var result = await _sut.GetDashboardKPIsAsync();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.StatusCode.Should().Be(200);

            var kpis = okResult.Value as DashboardMetricsDto;
            kpis.Should().NotBeNull();
            kpis!.TotalAvailableProperties.Should().Be(12);
            kpis.TotalReservedProperties.Should().Be(4);
            kpis.TotalSoldProperties.Should().Be(8);
            kpis.TotalProperties.Should().Be(24);
            kpis.TotalActiveAgents.Should().Be(5);
            kpis.TotalInactiveAgents.Should().Be(2);
            kpis.TotalClients.Should().Be(30);
            kpis.TotalDevelopers.Should().Be(3);
            kpis.PropertiesByType.Should().HaveCount(2);

            // Verificación crítica: NUNCA debe haber cargado entidades completas en memoria
            _propertyServiceMock.Verify(s => s.GetDashboardMetricsAsync(), Times.Once);
            _accountServiceMock.Verify(s => s.GetUserDashboardMetricsAsync(), Times.Once);
            _propertyServiceMock.Verify(s => s.GetAllViewModel(), Times.Never);
            _agentServiceMock.Verify(s => s.GetAllViewModelAsync(), Times.Never);
        }

        [Fact]
        public async Task GetDashboardKPIsAsync_DebeMapearCorrectamente_PropiedadesPorTipo()
        {
            // Arrange
            var propertyMetrics = new PropertyDashboardMetricsDto
            {
                TotalAvailableProperties = 1,
                TotalReservedProperties = 0,
                TotalSoldProperties = 0,
                TotalProperties = 1,
                PropertiesByType = new List<PropertyTypeCountDto>
                {
                    new() { TypeName = "Villa", Count = 1 }
                }
            };

            var userMetrics = new UserDashboardMetricsDto
            {
                TotalActiveAgents = 1,
                TotalInactiveAgents = 0,
                TotalClients = 1,
                TotalDevelopers = 1
            };

            _propertyServiceMock.Setup(s => s.GetDashboardMetricsAsync())
                .ReturnsAsync(propertyMetrics);
            _accountServiceMock.Setup(s => s.GetUserDashboardMetricsAsync())
                .ReturnsAsync(userMetrics);

            // Act
            var result = await _sut.GetDashboardKPIsAsync();

            // Assert
            var okResult = result as OkObjectResult;
            var kpis = okResult!.Value as DashboardMetricsDto;
            kpis!.PropertiesByType.Should().ContainSingle(p => p.TypeName == "Villa" && p.Count == 1);
        }
    }
}
