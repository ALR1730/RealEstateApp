using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Review;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly ReviewService _sut;

        public ReviewServiceTests()
        {
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            _accountServiceMock = new Mock<IAccountService>();

            // Default: batch lookup retorna diccionario vacío
            _accountServiceMock
                .Setup(a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, AccountUserDto>());

            _sut = new ReviewService(_reviewRepositoryMock.Object, _accountServiceMock.Object);
        }

        [Fact]
        public async Task SubmitReviewAsync_ConTransaccionCompletada_AddsReview()
        {
            // Arrange
            var vm = new SaveAgentReviewViewModel { AgentId = "agent-1", PropertyId = 10, Rating = 5, Comment = "Excelente" };
            _reviewRepositoryMock.Setup(r => r.HasCompletedTransactionAsync("client-1", "agent-1", 10)).ReturnsAsync(true);
            _reviewRepositoryMock.Setup(r => r.ExistsAsync("client-1", "agent-1", 10)).ReturnsAsync(false);
            _reviewRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AgentReview>()))
                .ReturnsAsync((AgentReview a) => { a.Id = 1; return a; });

            // Act
            await _sut.SubmitReviewAsync(vm, "client-1");

            // Assert
            _reviewRepositoryMock.Verify(r => r.AddAsync(It.Is<AgentReview>(a =>
                a.AgentId == "agent-1" &&
                a.ClienteId == "client-1" &&
                a.PropertyId == 10 &&
                a.Rating == 5)), Times.Once);
        }

        [Fact]
        public async Task SubmitReviewAsync_SinTransaccionCompletada_ThrowsValidationException()
        {
            // Arrange
            var vm = new SaveAgentReviewViewModel { AgentId = "agent-1", PropertyId = 10, Rating = 4 };
            _reviewRepositoryMock.Setup(r => r.HasCompletedTransactionAsync("client-1", "agent-1", 10)).ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _sut.SubmitReviewAsync(vm, "client-1");

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*después de completar una transacción*");
            _reviewRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AgentReview>()), Times.Never);
        }

        [Fact]
        public async Task SubmitReviewAsync_YaExisteResena_ThrowsValidationException()
        {
            // Arrange
            var vm = new SaveAgentReviewViewModel { AgentId = "agent-1", PropertyId = 10, Rating = 4 };
            _reviewRepositoryMock.Setup(r => r.HasCompletedTransactionAsync("client-1", "agent-1", 10)).ReturnsAsync(true);
            _reviewRepositoryMock.Setup(r => r.ExistsAsync("client-1", "agent-1", 10)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _sut.SubmitReviewAsync(vm, "client-1");

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Ya has calificado*");
        }

        [Fact]
        public async Task HasReviewedAsync_PremioResenado_ReturnsTrue()
        {
            // Arrange
            _reviewRepositoryMock.Setup(r => r.ExistsAsync("client-1", "agent-1", 10)).ReturnsAsync(true);

            // Act
            var result = await _sut.HasReviewedAsync("client-1", "agent-1", 10);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanReviewAsync_SinTransaccion_ReturnsFalse()
        {
            // Arrange
            _reviewRepositoryMock.Setup(r => r.ExistsAsync("client-1", "agent-1", 10)).ReturnsAsync(false);
            _reviewRepositoryMock.Setup(r => r.HasCompletedTransactionAsync("client-1", "agent-1", 10)).ReturnsAsync(false);

            // Act
            var result = await _sut.CanReviewAsync("client-1", "agent-1", 10);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanReviewAsync_YaResenado_ReturnsFalse()
        {
            // Arrange
            _reviewRepositoryMock.Setup(r => r.ExistsAsync("client-1", "agent-1", 10)).ReturnsAsync(true);
            _reviewRepositoryMock.Setup(r => r.HasCompletedTransactionAsync("client-1", "agent-1", 10)).ReturnsAsync(true);

            // Act
            var result = await _sut.CanReviewAsync("client-1", "agent-1", 10);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAgentReviewSummaryAsync_CalculaConteosYPromedio()
        {
            // Arrange
            var reviews = new List<AgentReview>
            {
                new() { AgentId = "agent-1", Rating = 5 },
                new() { AgentId = "agent-1", Rating = 5 },
                new() { AgentId = "agent-1", Rating = 4 },
                new() { AgentId = "agent-1", Rating = 1 }
            };
            _reviewRepositoryMock.Setup(r => r.GetByAgentIdAsync("agent-1")).ReturnsAsync(reviews);

            // Act
            var summary = await _sut.GetAgentReviewSummaryAsync("agent-1");

            // Assert
            summary.ReviewCount.Should().Be(4);
            summary.AverageRating.Should().Be(3.8);
            summary.FiveStars.Should().Be(2);
            summary.FourStars.Should().Be(1);
            summary.OneStar.Should().Be(1);
        }

        [Fact]
        public async Task GetReviewsByAgentAsync_DebeHacerBatchLookupDeClientes_SinN1()
        {
            // Arrange — 2 reviews del mismo agente con clientes distintos
            var reviews = new List<AgentReview>
            {
                new() { Id = 1, AgentId = "agent-1", ClienteId = "client-A", PropertyId = 10, Rating = 5,
                        Comment = "Muy profesional", Created = DateTime.UtcNow,
                        Property = new Property { Id = 10, Code = "ABA001", Name = "Villa Sol" } },
                new() { Id = 2, AgentId = "agent-1", ClienteId = "client-B", PropertyId = 11, Rating = 4,
                        Comment = "Buen servicio", Created = DateTime.UtcNow,
                        Property = new Property { Id = 11, Code = "ABA002", Name = "Piso Mar" } }
            };
            _reviewRepositoryMock.Setup(r => r.GetByAgentIdAsync("agent-1")).ReturnsAsync(reviews);

            var clientDict = new Dictionary<string, AccountUserDto>
            {
                ["client-A"] = new AccountUserDto { Id = "client-A", FirstName = "Ana",  LastName = "Lopez",   Email = "ana@test.com" },
                ["client-B"] = new AccountUserDto { Id = "client-B", FirstName = "Pedro", LastName = "Martín", Email = "pedro@test.com" }
            };

            _accountServiceMock
                .Setup(a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(clientDict);

            // Act
            var result = await _sut.GetReviewsByAgentAsync("agent-1");

            // Assert — estado observable correcto
            result.Should().HaveCount(2);
            result.First(r => r.ClienteId == "client-A").ClienteName.Should().Be("Ana Lopez");
            result.First(r => r.ClienteId == "client-B").ClienteName.Should().Be("Pedro Martín");

            // El batch lookup debe llamarse exactamente UNA VEZ para ambos clientes
            _accountServiceMock.Verify(
                a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()),
                Times.Once,
                "Debe hacerse exactamente 1 batch lookup para N reviews, eliminando el N+1");
        }
    }
}