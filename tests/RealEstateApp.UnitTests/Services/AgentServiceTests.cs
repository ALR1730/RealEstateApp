using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class AgentServiceTests
    {
        private readonly Mock<IPropertyRepository> _propertyRepoMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly Mock<IOfferRepository> _offerRepoMock;
        private readonly Mock<IChatRepository> _chatRepoMock;
        private readonly Mock<IFavoriteRepository> _favoriteRepoMock;
        private readonly Mock<IPropertyImageRepository> _propertyImageRepoMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly Mock<IAgentVerificationRepository> _verificationRepoMock;
        private readonly IMapper _mapper;
        private readonly AgentService _sut;

        public AgentServiceTests()
        {
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _accountServiceMock = new Mock<IAccountService>();
            _offerRepoMock = new Mock<IOfferRepository>();
            _chatRepoMock = new Mock<IChatRepository>();
            _favoriteRepoMock = new Mock<IFavoriteRepository>();
            _propertyImageRepoMock = new Mock<IPropertyImageRepository>();
            _fileStorageMock = new Mock<IFileStorageService>();
            _verificationRepoMock = new Mock<IAgentVerificationRepository>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _sut = new AgentService(
                _propertyRepoMock.Object,
                _accountServiceMock.Object,
                _offerRepoMock.Object,
                _chatRepoMock.Object,
                _favoriteRepoMock.Object,
                _propertyImageRepoMock.Object,
                _fileStorageMock.Object,
                _verificationRepoMock.Object,
                _mapper);
        }

        [Fact]
        public async Task GetAllViewModelAsync_DebeRetornarAgentesConCantidadDePropiedadesYVerificacion()
        {
            // Arrange
            var agents = new List<AccountUserDto>
            {
                new AccountUserDto
                {
                    Id = "agent-1",
                    FirstName = "Ana",
                    LastName = "Gomez",
                    Email = "ana@realty.com",
                    IsActive = true
                }
            };

            var properties = new List<Property>
            {
                new Property { Id = 1, AgentId = "agent-1", Status = RealEstateApp.Core.Domain.Constants.PropertyStatus.Available },
                new Property { Id = 2, AgentId = "agent-1", Status = RealEstateApp.Core.Domain.Constants.PropertyStatus.Available }
            };

            var verifications = new List<AgentVerification>
            {
                new AgentVerification { AgentId = "agent-1", Status = RealEstateApp.Core.Domain.Constants.VerificationStatus.Approved }
            };

            _accountServiceMock.Setup(a => a.GetUsersInRoleAsync("Agent"))
                .ReturnsAsync(agents);

            _propertyRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(properties);

            _verificationRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(verifications);

            // Act
            var result = await _sut.GetAllViewModelAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be("agent-1");
            result[0].PropertiesCount.Should().Be(2);
            result[0].IsVerified.Should().BeTrue();
        }

        [Fact]
        public async Task ChangeStatusAsync_DebeInvocarAccountService()
        {
            // Arrange
            _accountServiceMock.Setup(a => a.ChangeUserStatusAsync("agent-1", false))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.ChangeStatusAsync("agent-1", false);

            // Assert
            _accountServiceMock.Verify(a => a.ChangeUserStatusAsync("agent-1", false), Times.Once);
        }

        [Fact]
        public async Task DeleteAgentCascadeAsync_DebeEjecutarEliminacionDentroDeTransaccionAtomica()
        {
            // Arrange
            var agentId = "agent-to-delete";
            var agentUser = new AccountUserDto { Id = agentId, FirstName = "Carlos", LastName = "Mendoza" };
            var properties = new List<Property>
            {
                new() { Id = 101, AgentId = agentId }
            };

            _accountServiceMock.Setup(a => a.GetUserByIdAsync(agentId))
                .ReturnsAsync(agentUser);
            _propertyRepoMock.Setup(r => r.GetByAgentIdAsync(agentId))
                .ReturnsAsync(properties);
            _propertyImageRepoMock.Setup(r => r.GetByPropertyIdAsync(101))
                .ReturnsAsync(new List<PropertyImage>());
            _favoriteRepoMock.Setup(r => r.GetByPropertyIdAsync(101))
                .ReturnsAsync(new List<Favorite>());
            _offerRepoMock.Setup(r => r.GetByPropertyIdAsync(101))
                .ReturnsAsync(new List<Offer>());
            _chatRepoMock.Setup(r => r.GetByPropertyIdAsync(101))
                .ReturnsAsync(new List<Chat>());
            _chatRepoMock.Setup(r => r.GetByUserIdAsync(agentId))
                .ReturnsAsync(new List<Chat>());

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default))
                .Returns<Func<Task>, System.Threading.CancellationToken>(async (action, _) => await action());

            var sutWithUow = new AgentService(
                _propertyRepoMock.Object,
                _accountServiceMock.Object,
                _offerRepoMock.Object,
                _chatRepoMock.Object,
                _favoriteRepoMock.Object,
                _propertyImageRepoMock.Object,
                _fileStorageMock.Object,
                _verificationRepoMock.Object,
                _mapper,
                unitOfWorkMock.Object
            );

            // Act
            await sutWithUow.DeleteAgentCascadeAsync(agentId);

            // Assert
            unitOfWorkMock.Verify(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), default), Times.Once);
            _propertyRepoMock.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<Property>>(list => list == properties)), Times.Once);
            _accountServiceMock.Verify(a => a.DeleteUserAsync(agentId), Times.Once);
        }
    }
}
