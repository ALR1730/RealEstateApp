using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
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
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly ReviewService _sut;

        public ReviewServiceTests()
        {
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            var store = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _sut = new ReviewService(_reviewRepositoryMock.Object, _userManagerMock.Object);
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
                new AgentReview { AgentId = "agent-1", Rating = 5 },
                new AgentReview { AgentId = "agent-1", Rating = 5 },
                new AgentReview { AgentId = "agent-1", Rating = 4 },
                new AgentReview { AgentId = "agent-1", Rating = 1 }
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
        public async Task GetReviewsByAgentAsync_ResuelveIdentidadDelCliente()
        {
            // Arrange
            var review = new AgentReview
            {
                Id = 7,
                AgentId = "agent-1",
                ClienteId = "client-1",
                PropertyId = 10,
                Rating = 5,
                Comment = "Muy profesional",
                Created = DateTime.UtcNow,
                Property = new Property { Id = 10, Code = "ABA001", Name = "Villa Sol" }
            };
            _reviewRepositoryMock.Setup(r => r.GetByAgentIdAsync("agent-1")).ReturnsAsync(new List<AgentReview> { review });

            var cliente = new IdentityUser { Id = "client-1", UserName = "cliente1", Email = "cliente@test.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync("client-1")).ReturnsAsync(cliente);
            _userManagerMock.Setup(u => u.GetClaimsAsync(cliente)).ReturnsAsync(new List<Claim>
            {
                new("FirstName", "Ana"),
                new("LastName", "Lopez")
            });

            // Act
            var result = await _sut.GetReviewsByAgentAsync("agent-1");

            // Assert
            var vm = result.Should().ContainSingle().Subject;
            vm.PropertyCode.Should().Be("ABA001");
            vm.PropertyName.Should().Be("Villa Sol");
            vm.ClienteName.Should().Be("Ana Lopez");
            vm.ClienteEmail.Should().Be("cliente@test.com");
        }
    }
}