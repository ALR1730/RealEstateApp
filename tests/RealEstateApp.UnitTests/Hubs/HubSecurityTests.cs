using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Presentation.WebApi.Hubs;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Hubs
{
    public class HubSecurityTests
    {
        private readonly Mock<IChatRepository> _chatRepoMock;
        private readonly Mock<IPropertyRepository> _propertyRepoMock;
        private readonly IMapper _mapper;
        private readonly ChatService _chatService;

        public HubSecurityTests()
        {
            _chatRepoMock = new Mock<IChatRepository>();
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _chatService = new ChatService(
                _chatRepoMock.Object,
                _propertyRepoMock.Object,
                _mapper);
        }

        #region NotificationHub Surface Security

        [Fact]
        public void NotificationHub_NoDebeExponerMetodoPublicoSendNotificationToUser()
        {
            // Arrange
            var hubType = typeof(NotificationHub);

            // Act
            var publicMethod = hubType.GetMethod("SendNotificationToUser");
            var anyMatchingMethod = hubType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .FirstOrDefault(m => string.Equals(m.Name, "SendNotificationToUser", StringComparison.OrdinalIgnoreCase));

            // Assert
            publicMethod.Should().BeNull(
                "los clientes SignalR nunca deben poder invocar SendNotificationToUser directamente para suplantar notificaciones");
            anyMatchingMethod.Should().BeNull();
        }

        #endregion

        #region ChatService Input & Authorization Validation

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        [InlineData(null)]
        public async Task SendMessage_DebeLanzarValidationException_CuandoMensajeEsVacioONulo(string? emptyMessage)
        {
            // Arrange
            var property = new Property { Id = 10, AgentId = "agent-owner" };
            _propertyRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(property);

            var vm = new SaveChatViewModel
            {
                PropertyId = 10,
                RecipientId = "agent-owner",
                MessageContent = emptyMessage!
            };

            // Act
            Func<Task> act = () => _chatService.SendMessage(vm, senderId: "client-1");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
            _chatRepoMock.Verify(r => r.AddAsync(It.IsAny<Chat>()), Times.Never);
        }

        [Fact]
        public async Task SendMessage_DebeLanzarValidationException_CuandoMensajeExcede2000Caracteres()
        {
            // Arrange
            var property = new Property { Id = 10, AgentId = "agent-owner" };
            _propertyRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(property);

            var vm = new SaveChatViewModel
            {
                PropertyId = 10,
                RecipientId = "agent-owner",
                MessageContent = new string('a', 2001)
            };

            // Act
            Func<Task> act = () => _chatService.SendMessage(vm, senderId: "client-1");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
            _chatRepoMock.Verify(r => r.AddAsync(It.IsAny<Chat>()), Times.Never);
        }

        [Fact]
        public async Task SendMessage_DebeLanzarValidationException_CuandoNiEmisorNiReceptorSonAgenteDeLaPropiedad()
        {
            // Arrange
            var property = new Property { Id = 10, AgentId = "agent-owner" };
            _propertyRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(property);

            var vm = new SaveChatViewModel
            {
                PropertyId = 10,
                RecipientId = "attacker-recipient",
                MessageContent = "Intento de inyección en hilo ajeno"
            };

            // Act
            Func<Task> act = () => _chatService.SendMessage(vm, senderId: "attacker-sender");

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
            _chatRepoMock.Verify(r => r.AddAsync(It.IsAny<Chat>()), Times.Never);
        }

        [Fact]
        public async Task SendMessage_DebeLanzarDomainException_CuandoPropiedadEspecificadaNoExiste()
        {
            // Arrange
            _propertyRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Property?)null);

            var vm = new SaveChatViewModel
            {
                PropertyId = 999,
                RecipientId = "agent-owner",
                MessageContent = "Hola, ¿sigue disponible?"
            };

            // Act
            Func<Task> act = () => _chatService.SendMessage(vm, senderId: "client-1");

            // Assert
            await act.Should().ThrowAsync<DomainException>();
            _chatRepoMock.Verify(r => r.AddAsync(It.IsAny<Chat>()), Times.Never);
        }

        [Fact]
        public async Task SendMessage_DebePermitirEnvioConTimestampUtc_CuandoMensajeTiene2000CaracteresYParticipaAgente()
        {
            // Arrange
            var property = new Property { Id = 10, AgentId = "agent-owner" };
            _propertyRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(property);

            var maxAllowedMessage = new string('x', 2000);
            var vm = new SaveChatViewModel
            {
                PropertyId = 10,
                RecipientId = "agent-owner",
                MessageContent = maxAllowedMessage
            };

            // Act
            Func<Task> act = () => _chatService.SendMessage(vm, senderId: "client-1");

            // Assert
            await act.Should().NotThrowAsync();
            _chatRepoMock.Verify(r => r.AddAsync(It.Is<Chat>(c =>
                c.PropertyId == 10 &&
                c.ClienteId == "client-1" &&
                c.AgenteId == "agent-owner" &&
                c.SenderId == "client-1" &&
                c.SentAt.Kind == DateTimeKind.Utc)), Times.Once);
        }

        [Fact]
        public async Task SendMessage_DebePermitirEnvio_CuandoEmisorEsElAgenteDeLaPropiedad()
        {
            // Arrange
            var property = new Property { Id = 10, AgentId = "agent-owner" };
            _propertyRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(property);

            var vm = new SaveChatViewModel
            {
                PropertyId = 10,
                RecipientId = "client-1",
                MessageContent = "Sí, está disponible para visita mañana."
            };

            // Act
            Func<Task> act = () => _chatService.SendMessage(vm, senderId: "agent-owner");

            // Assert
            await act.Should().NotThrowAsync();
            _chatRepoMock.Verify(r => r.AddAsync(It.Is<Chat>(c =>
                c.PropertyId == 10 &&
                c.ClienteId == "client-1" &&
                c.AgenteId == "agent-owner" &&
                c.SenderId == "agent-owner" &&
                c.SentAt.Kind == DateTimeKind.Utc)), Times.Once);
        }

        #endregion
    }
}
