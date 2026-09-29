using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatRepository> _chatRepoMock;
        private readonly Mock<IPropertyRepository> _propertyRepoMock;
        private readonly IMapper _mapper;
        private readonly ChatService _sut;

        public ChatServiceTests()
        {
            _chatRepoMock = new Mock<IChatRepository>();
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _sut = new ChatService(
                _chatRepoMock.Object,
                _propertyRepoMock.Object,
                _mapper);
        }

        [Fact]
        public async Task SendMessage_DebeSanitizarContenidoYGuardarMensaje_CuandoEmisorEsCliente()
        {
            // Arrange
            var property = new Property { Id = 10, AgentId = "agent-10" };
            _propertyRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(property);

            var vm = new SaveChatViewModel
            {
                PropertyId = 10,
                RecipientId = "agent-10",
                MessageContent = "<script>alert('xss')</script>Hola me interesa"
            };

            // Act
            await _sut.SendMessage(vm, senderId: "client-5");

            // Assert
            _chatRepoMock.Verify(r => r.AddAsync(It.Is<Chat>(c =>
                c.ClienteId == "client-5" &&
                c.AgenteId == "agent-10" &&
                c.SenderId == "client-5" &&
                !c.MessageContent.Contains("<script>")
            )), Times.Once);
        }

        [Fact]
        public async Task GetChatThread_DebeMarcarIsMineTrue_ParaMensajesDelUsuarioActual()
        {
            // Arrange
            var chats = new List<Chat>
            {
                new Chat { Id = 1, SenderId = "user-1", MessageContent = "Hola" },
                new Chat { Id = 2, SenderId = "user-2", MessageContent = "Hola, ¿en qué te ayudo?" }
            };

            _chatRepoMock.Setup(r => r.GetChatThreadAsync("user-1", "user-2", 10))
                .ReturnsAsync(chats);

            // Act
            var thread = await _sut.GetChatThread("user-1", "user-2", 10, currentUserId: "user-1");

            // Assert
            thread.Should().HaveCount(2);
            thread[0].IsMine.Should().BeTrue();
            thread[1].IsMine.Should().BeFalse();
        }
    }
}
