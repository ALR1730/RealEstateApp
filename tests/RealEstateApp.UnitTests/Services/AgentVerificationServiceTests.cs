using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class AgentVerificationServiceTests
    {
        private readonly Mock<IAgentVerificationRepository> _verificationRepoMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly AgentVerificationService _sut;

        public AgentVerificationServiceTests()
        {
            _verificationRepoMock = new Mock<IAgentVerificationRepository>();
            _fileStorageMock = new Mock<IFileStorageService>();
            _accountServiceMock = new Mock<IAccountService>();

            // Default: batch lookup retorna diccionario vacío (no afecta tests que no consultan usuarios)
            _accountServiceMock
                .Setup(a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, AccountUserDto>());

            _sut = new AgentVerificationService(
                _verificationRepoMock.Object,
                _fileStorageMock.Object,
                _accountServiceMock.Object);
        }

        [Fact]
        public async Task SubmitVerificationAsync_DebeCrearNuevaSolicitud_CuandoNoExistePrevia()
        {
            // Arrange
            var vm = new AgentVerificationViewModel
            {
                AgentId = "agent-1",
                Cedula = "402-0000000-1"
            };

            _verificationRepoMock.Setup(r => r.GetByAgentIdAsync("agent-1"))
                .ReturnsAsync((AgentVerification?)null);

            _verificationRepoMock.Setup(r => r.AddAsync(It.IsAny<AgentVerification>()))
                .ReturnsAsync((AgentVerification v) => { v.Id = 1; return v; });

            // Act
            var result = await _sut.SubmitVerificationAsync(vm);

            // Assert
            result.Should().BeTrue();
            _verificationRepoMock.Verify(r => r.AddAsync(It.Is<AgentVerification>(v =>
                v.AgentId == "agent-1" &&
                v.Cedula == "402-0000000-1" &&
                v.Status == VerificationStatus.Pending
            )), Times.Once);
        }

        [Fact]
        public async Task SubmitVerificationAsync_DebeActualizarAEstadoPendiente_CuandoExisteSolicitudPrevia()
        {
            // Arrange
            var existing = new AgentVerification
            {
                Id = 1,
                AgentId = "agent-1",
                Cedula = "001-0000000-1",
                Status = VerificationStatus.Rejected,
                RejectionReason = "Foto borrosa"
            };

            var vm = new AgentVerificationViewModel
            {
                AgentId = "agent-1",
                Cedula = "402-1234567-8"
            };

            _verificationRepoMock.Setup(r => r.GetByAgentIdAsync("agent-1"))
                .ReturnsAsync(existing);

            _verificationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<AgentVerification>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.SubmitVerificationAsync(vm);

            // Assert
            result.Should().BeTrue();
            existing.Status.Should().Be(VerificationStatus.Pending);
            existing.Cedula.Should().Be("402-1234567-8");
            existing.RejectionReason.Should().BeNull();
            _verificationRepoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        [Fact]
        public async Task ReviewVerificationAsync_DebeAprobarSolicitud_CuandoApprovedEsTrue()
        {
            // Arrange
            var entity = new AgentVerification
            {
                Id = 5,
                AgentId = "agent-1",
                Status = VerificationStatus.Pending
            };

            _verificationRepoMock.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(entity);

            _verificationRepoMock.Setup(r => r.UpdateAsync(It.IsAny<AgentVerification>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ReviewVerificationAsync(5, approved: true, rejectionReason: null, adminId: "admin-99");

            // Assert
            result.Should().BeTrue();
            entity.Status.Should().Be(VerificationStatus.Approved);
            entity.ReviewedByAdminId.Should().Be("admin-99");
            entity.RejectionReason.Should().BeNull();
            entity.ReviewedAt.Should().NotBeNull();
            _verificationRepoMock.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task ReviewVerificationAsync_DebeRechazarConMotivo_CuandoApprovedEsFalse()
        {
            // Arrange
            var entity = new AgentVerification
            {
                Id = 8,
                AgentId = "agent-2",
                Status = VerificationStatus.Pending
            };

            _verificationRepoMock.Setup(r => r.GetByIdAsync(8))
                .ReturnsAsync(entity);

            // Act
            var result = await _sut.ReviewVerificationAsync(8, approved: false, rejectionReason: "Documento ilegible", adminId: "admin-1");

            // Assert
            result.Should().BeTrue();
            entity.Status.Should().Be(VerificationStatus.Rejected);
            entity.RejectionReason.Should().Be("Documento ilegible");
            entity.ReviewedByAdminId.Should().Be("admin-1");
            _verificationRepoMock.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task IsAgentVerifiedAsync_DebeRetornarTrue_CuandoElEstadoEsApproved()
        {
            // Arrange
            var entity = new AgentVerification
            {
                AgentId = "agent-verified",
                Status = VerificationStatus.Approved
            };

            _verificationRepoMock.Setup(r => r.GetByAgentIdAsync("agent-verified"))
                .ReturnsAsync(entity);

            // Act
            var result = await _sut.IsAgentVerifiedAsync("agent-verified");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAgentVerifiedAsync_DebeRetornarFalse_CuandoElEstadoEsPendingOInexistente()
        {
            // Arrange
            _verificationRepoMock.Setup(r => r.GetByAgentIdAsync("agent-unknown"))
                .ReturnsAsync((AgentVerification?)null);

            // Act
            var result = await _sut.IsAgentVerifiedAsync("agent-unknown");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_DebeHacerUnSoloBatchLookup_SinImportarCantidadDeVerificaciones()
        {
            // Arrange — 3 verificaciones con 2 agentes distintos y 1 admin compartido
            var entities = new List<AgentVerification>
            {
                new() { Id = 1, AgentId = "agent-A", ReviewedByAdminId = "admin-1", Status = VerificationStatus.Approved },
                new() { Id = 2, AgentId = "agent-B", ReviewedByAdminId = "admin-1", Status = VerificationStatus.Rejected },
                new() { Id = 3, AgentId = "agent-A", ReviewedByAdminId = null,      Status = VerificationStatus.Pending }
            };

            _verificationRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

            var userDict = new Dictionary<string, AccountUserDto>
            {
                ["agent-A"] = new AccountUserDto { Id = "agent-A", FirstName = "Ana",   LastName = "García", Email = "ana@test.com" },
                ["agent-B"] = new AccountUserDto { Id = "agent-B", FirstName = "Luis",  LastName = "Pérez",  Email = "luis@test.com" },
                ["admin-1"] = new AccountUserDto { Id = "admin-1", FirstName = "Carlos", LastName = "Admin", Email = "admin@test.com" }
            };

            _accountServiceMock
                .Setup(a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(userDict);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert — estado observable: datos mapeados correctamente
            result.Should().HaveCount(3);
            result[0].AgentName.Should().Be("Ana García");
            result[1].AgentName.Should().Be("Luis Pérez");

            // Verificar que el batch lookup se llamó UNA SOLA VEZ (no 3 veces)
            _accountServiceMock.Verify(
                a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()),
                Times.Once,
                "El batch lookup debe ejecutarse exactamente una vez para todas las verificaciones");
        }
    }
}
