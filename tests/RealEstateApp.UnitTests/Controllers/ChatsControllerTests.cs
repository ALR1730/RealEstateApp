using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    /// <summary>
    /// Pruebas de seguridad para ChatsController.
    /// Verifican la protección BOLA/IDOR (OWASP API Security Top 10 — API1).
    /// </summary>
    public class ChatsControllerTests
    {
        private readonly Mock<IChatService> _chatServiceMock;
        private readonly ChatsController _sut;

        public ChatsControllerTests()
        {
            _chatServiceMock = new Mock<IChatService>();
            _sut = new ChatsController(_chatServiceMock.Object);
        }

        /// <summary>
        /// Helper: configura el HttpContext del controlador con la identidad del userId indicado.
        /// </summary>
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

        #region BOLA/IDOR Protection — GetThreadAsync (Hallazgo 2.1)

        /// <summary>
        /// Hallazgo 2.1 (CRÍTICO): un usuario que NO es participante del hilo
        /// debe recibir 403 Forbidden. Sin esta guarda, cualquier usuario autenticado
        /// puede leer conversaciones privadas entre otros usuarios.
        /// </summary>
        [Fact]
        public async Task GetThreadAsync_DebeRetornar403_CuandoUsuarioNoEsParticipante()
        {
            // Arrange — espía que no pertenece al hilo cliente-123 / agente-456
            SetAuthenticatedUser("intruso-999", "Client");

            // Act
            var result = await _sut.GetThreadAsync(clientId: "cliente-123", agentId: "agente-456", propertyId: 1);

            // Assert
            result.Should().BeOfType<ForbidResult>(
                "un usuario externo al hilo no debe poder leer conversaciones ajenas");
            _chatServiceMock.Verify(s => s.GetChatThread(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>()),
                Times.Never, "el servicio no debe ser invocado si el acceso está prohibido");
        }

        /// <summary>
        /// El cliente que inició la conversación (clientId) sí puede leer el hilo.
        /// </summary>
        [Fact]
        public async Task GetThreadAsync_DebePermitirAcceso_CuandoUsuarioEsElCliente()
        {
            // Arrange
            SetAuthenticatedUser("cliente-123", "Client");
            _chatServiceMock
                .Setup(s => s.GetChatThread("cliente-123", "agente-456", 1, "cliente-123"))
                .ReturnsAsync(new List<ChatViewModel>());

            // Act
            var result = await _sut.GetThreadAsync(clientId: "cliente-123", agentId: "agente-456", propertyId: 1);

            // Assert
            result.Should().BeOfType<OkObjectResult>("el cliente participante debe poder acceder a su hilo");
        }

        /// <summary>
        /// El agente que participa en la conversación (agentId) sí puede leer el hilo.
        /// </summary>
        [Fact]
        public async Task GetThreadAsync_DebePermitirAcceso_CuandoUsuarioEsElAgente()
        {
            // Arrange
            SetAuthenticatedUser("agente-456", "Agent");
            _chatServiceMock
                .Setup(s => s.GetChatThread("cliente-123", "agente-456", 1, "agente-456"))
                .ReturnsAsync(new List<ChatViewModel>());

            // Act
            var result = await _sut.GetThreadAsync(clientId: "cliente-123", agentId: "agente-456", propertyId: 1);

            // Assert
            result.Should().BeOfType<OkObjectResult>("el agente participante debe poder acceder al hilo");
        }

        /// <summary>
        /// Un Administrador puede ver cualquier hilo de conversación para supervisión.
        /// </summary>
        [Fact]
        public async Task GetThreadAsync_DebePermitirAcceso_CuandoUsuarioEsAdmin()
        {
            // Arrange
            SetAuthenticatedUser("admin-001", "Admin");
            _chatServiceMock
                .Setup(s => s.GetChatThread("cliente-123", "agente-456", 1, "admin-001"))
                .ReturnsAsync(new List<ChatViewModel>());

            // Act
            var result = await _sut.GetThreadAsync(clientId: "cliente-123", agentId: "agente-456", propertyId: 1);

            // Assert
            result.Should().BeOfType<OkObjectResult>("un Admin debe poder supervisar cualquier conversación");
        }

        #endregion
    }
}
