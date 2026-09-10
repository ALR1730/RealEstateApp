using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;
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

        [Fact]
        public async Task ConfirmEmail_ValidUserAndToken_ReturnsOkWithMessage()
        {
            // Arrange
            _accountServiceMock.Setup(s => s.ConfirmAccountAsync("user-1", "encoded-token"))
                .ReturnsAsync("Cuenta confirmada exitosamente para client@realestate.com. Ya puede iniciar sesión.");

            // Act
            var result = await _controller.ConfirmEmailAsync("user-1", "encoded-token");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(new
            {
                message = "Cuenta confirmada exitosamente para client@realestate.com. Ya puede iniciar sesión."
            });
        }

        [Fact]
        public async Task ConfirmEmail_UnknownUser_ThrowsNotFoundExceptionForFilter404()
        {
            // Arrange
            _accountServiceMock.Setup(s => s.ConfirmAccountAsync("ghost", "token"))
                .ThrowsAsync(new NotFoundException("No existe ningún usuario registrado con este ID"));

            // Act
            var act = async () => await _controller.ConfirmEmailAsync("ghost", "token");

            // Assert: la excepción se propaga y ApiGlobalExceptionFilter la mapea a 404.
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ConfirmEmail_InvalidToken_ThrowsValidationExceptionForFilter400()
        {
            // Arrange
            _accountServiceMock.Setup(s => s.ConfirmAccountAsync("user-1", "bad-token"))
                .ThrowsAsync(new ValidationException("Error al confirmar la cuenta para client@realestate.com: token inválido"));

            // Act
            var act = async () => await _controller.ConfirmEmailAsync("user-1", "bad-token");

            // Assert: la excepción se propaga y ApiGlobalExceptionFilter la mapea a 400.
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
