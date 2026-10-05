using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class AppointmentsControllerTests
    {
        private readonly Mock<IAppointmentService> _appointmentServiceMock;
        private readonly AppointmentsController _sut;

        public AppointmentsControllerTests()
        {
            _appointmentServiceMock = new Mock<IAppointmentService>();
            _sut = new AppointmentsController(_appointmentServiceMock.Object);
        }

        private void SetAuthenticatedUser(string userId, params string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task CompleteAppointmentAsync_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _sut.CompleteAppointmentAsync(1, "Notas de prueba");

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task CompleteAppointmentAsync_ShouldInvokeServiceAndReturnOk_WhenAgentIsAuthenticated()
        {
            // Arrange
            const string agentId = "agent-xyz-123";
            const int appointmentId = 42;
            const string notes = "Cliente satisfecho con la visita.";
            SetAuthenticatedUser(agentId, "Agent");

            _appointmentServiceMock
                .Setup(s => s.CompleteAppointmentAsync(appointmentId, agentId, notes))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CompleteAppointmentAsync(appointmentId, notes);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _appointmentServiceMock.Verify(s => s.CompleteAppointmentAsync(appointmentId, agentId, notes), Times.Once);
        }
    }
}
