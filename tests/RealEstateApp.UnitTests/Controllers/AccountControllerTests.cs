using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _accountServiceMock = new Mock<IAccountService>();
            _controller = new AccountController(_accountServiceMock.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var request = new AuthenticationRequest
            {
                Email = "agent@realestate.com",
                Password = "Password123!"
            };

            var authResponse = new AuthenticationResponse
            {
                Id = "agent-123",
                Email = "agent@realestate.com",
                UserName = "agentuser",
                JWToken = "mocked-jwt-token",
                Roles = new List<string> { Roles.Agent.ToString() },
                HasError = false
            };

            _accountServiceMock.Setup(s => s.AuthenticateAsync(request))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.AuthenticateAsync(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(authResponse);
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var request = new AuthenticationRequest
            {
                Email = "wrong@realestate.com",
                Password = "WrongPassword"
            };

            var authResponse = new AuthenticationResponse
            {
                HasError = true,
                Error = "Credenciales incorrectas."
            };

            _accountServiceMock.Setup(s => s.AuthenticateAsync(request))
                .ReturnsAsync(authResponse);

            // Act
            var result = await _controller.AuthenticateAsync(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
