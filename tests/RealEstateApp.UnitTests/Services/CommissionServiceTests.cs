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
using RealEstateApp.Core.Application.ViewModels.Subscription;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class CommissionServiceTests
    {
        private readonly Mock<ICommissionRepository> _commissionRepoMock;
        private readonly Mock<IOfferRepository> _offerRepoMock;
        private readonly Mock<IPropertyRepository> _propertyRepoMock;
        private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
        private readonly Mock<IAccountService> _accountServiceMock;
        private readonly CommissionService _sut;

        public CommissionServiceTests()
        {
            _commissionRepoMock = new Mock<ICommissionRepository>();
            _offerRepoMock = new Mock<IOfferRepository>();
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _subscriptionServiceMock = new Mock<ISubscriptionService>();
            _accountServiceMock = new Mock<IAccountService>();

            _accountServiceMock
                .Setup(a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new Dictionary<string, AccountUserDto>());

            _sut = new CommissionService(
                _commissionRepoMock.Object,
                _offerRepoMock.Object,
                _propertyRepoMock.Object,
                _subscriptionServiceMock.Object,
                _accountServiceMock.Object);
        }

        [Fact]
        public async Task CreateForAcceptedOfferAsync_DebeCrearComision_CuandoLaOfertaExiste()
        {
            // Arrange
            var offer = new Offer { Id = 10, PropertyId = 5, MontoOfertado = 2_000_000m };
            var property = new Property { Id = 5, AgentId = "agent-1" };

            _offerRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(offer);
            _propertyRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);
            _commissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Commission>());
            _subscriptionServiceMock
                .Setup(s => s.GetCurrentSubscriptionByAgentIdAsync("agent-1"))
                .ReturnsAsync((AgentSubscriptionViewModel?)null);
            _commissionRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Commission>()))
                .ReturnsAsync((Commission c) => c);

            // Act
            await _sut.CreateForAcceptedOfferAsync(10);

            // Assert — verifica que se crea la comisión con la tasa default
            _commissionRepoMock.Verify(r => r.AddAsync(It.Is<Commission>(c =>
                c.AgentId == "agent-1" &&
                c.PropertyId == 5 &&
                c.OfferId == 10 &&
                c.Rate == CommissionConstants.DefaultRate &&
                c.Amount == Math.Round(2_000_000m * CommissionConstants.DefaultRate / 100m, 2)
            )), Times.Once);
        }

        [Fact]
        public async Task CreateForAcceptedOfferAsync_DebeIgnorar_CuandoYaExisteComisionParaLaOferta()
        {
            // Arrange
            var offer = new Offer { Id = 10, PropertyId = 5, MontoOfertado = 2_000_000m };
            var property = new Property { Id = 5, AgentId = "agent-1" };
            var existing = new List<Commission> { new() { OfferId = 10 } };

            _offerRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(offer);
            _propertyRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);
            _commissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(existing);

            // Act
            await _sut.CreateForAcceptedOfferAsync(10);

            // Assert — NO debe crear otra comisión duplicada
            _commissionRepoMock.Verify(r => r.AddAsync(It.IsAny<Commission>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_DebeHacerBatchLookupDeAgentes_SinN1()
        {
            // Arrange — 3 comisiones con 2 agentes distintos
            var commissions = new List<Commission>
            {
                new() { Id = 1, AgentId = "agent-A", PropertyId = 1, OfferId = 1, SalePrice = 1_000_000m, Rate = 3m, Amount = 30_000m, Status = CommissionStatus.Pending },
                new() { Id = 2, AgentId = "agent-B", PropertyId = 2, OfferId = 2, SalePrice = 2_000_000m, Rate = 3m, Amount = 60_000m, Status = CommissionStatus.Paid },
                new() { Id = 3, AgentId = "agent-A", PropertyId = 3, OfferId = 3, SalePrice = 1_500_000m, Rate = 3m, Amount = 45_000m, Status = CommissionStatus.Pending }
            };

            _commissionRepoMock.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(commissions);

            var agentDict = new Dictionary<string, AccountUserDto>
            {
                ["agent-A"] = new AccountUserDto { Id = "agent-A", FirstName = "Pedro", LastName = "Soto", Email = "pedro@test.com" },
                ["agent-B"] = new AccountUserDto { Id = "agent-B", FirstName = "María", LastName = "López", Email = "maria@test.com" }
            };

            _accountServiceMock
                .Setup(a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(agentDict);

            // Act
            var result = await _sut.GetAllAsync();

            // Assert — datos correctamente mapeados desde el batch
            result.Should().HaveCount(3);
            result.First(c => c.AgentId == "agent-A").AgentName.Should().Be("Pedro Soto");
            result.First(c => c.AgentId == "agent-B").AgentName.Should().Be("María López");

            // El batch lookup debe llamarse exactamente UNA VEZ
            _accountServiceMock.Verify(
                a => a.GetUsersByIdsAsync(It.IsAny<IEnumerable<string>>()),
                Times.Once,
                "Debe hacerse un único batch lookup, no N queries individuales");
        }

        [Fact]
        public async Task MarkAsPaidAsync_DebeActualizarEstado_CuandoComisionExiste()
        {
            // Arrange
            var commission = new Commission
            {
                Id = 1,
                AgentId = "agent-1",
                Amount = 30_000m,
                Status = CommissionStatus.Pending
            };

            _commissionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(commission);
            _commissionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Commission>())).Returns(Task.CompletedTask);

            // Act
            await _sut.MarkAsPaidAsync(1);

            // Assert — verificar ESTADO final, no llamada al mock
            commission.Status.Should().Be(CommissionStatus.Paid);
            _commissionRepoMock.Verify(r => r.UpdateAsync(commission), Times.Once);
        }

        [Fact]
        public async Task GetAgentSummaryAsync_DebeCalcularTotalesCorrectamente()
        {
            // Arrange
            var commissions = new List<Commission>
            {
                new() { AgentId = "agent-1", Amount = 30_000m, Rate = 3m, Status = CommissionStatus.Paid },
                new() { AgentId = "agent-1", Amount = 60_000m, Rate = 3m, Status = CommissionStatus.Pending },
                new() { AgentId = "agent-1", Amount = 45_000m, Rate = 3m, Status = CommissionStatus.Paid }
            };

            _commissionRepoMock.Setup(r => r.GetByAgentIdAsync("agent-1")).ReturnsAsync(commissions);

            // Act
            var summary = await _sut.GetAgentSummaryAsync("agent-1");

            // Assert
            summary.TotalCount.Should().Be(3);
            summary.TotalAmount.Should().Be(135_000m);
            summary.PaidCount.Should().Be(2);
            summary.PaidAmount.Should().Be(75_000m);
            summary.PendingCount.Should().Be(1);
            summary.PendingAmount.Should().Be(60_000m);
        }

        /// <summary>
        /// Tarea 1.1 — Hallazgo CRÍTICO: La comisión debe calcularse sobre el precio final pactado.
        /// Cuando el agente emite una contraoferta y el cliente la acepta, el precio real de cierre
        /// es CounterOfferAmount, no MontoOfertado.
        /// </summary>
        [Fact]
        public async Task CreateForAcceptedOfferAsync_DebeCalcularComisionSobreMontoContraoferta_CuandoOfertaTieneContraofertaAceptada()
        {
            // Arrange
            // Cliente ofertó RD$ 5,000,000. Agente contraofertó RD$ 5,500,000. Cliente aceptó.
            // El precio de cierre REAL es RD$ 5,500,000 (CounterOfferAmount).
            var offer = new Offer
            {
                Id = 99,
                PropertyId = 7,
                MontoOfertado = 5_000_000m,       // Oferta original del cliente
                CounterOfferAmount = 5_500_000m   // Contraoferta del agente (precio REAL de cierre)
            };
            var property = new Property { Id = 7, AgentId = "agent-A" };

            _offerRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(offer);
            _propertyRepoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(property);
            _commissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Commission>());
            _subscriptionServiceMock
                .Setup(s => s.GetCurrentSubscriptionByAgentIdAsync("agent-A"))
                .ReturnsAsync((AgentSubscriptionViewModel?)null); // Usará tasa default
            _commissionRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Commission>()))
                .ReturnsAsync((Commission c) => c);

            // Act
            await _sut.CreateForAcceptedOfferAsync(99);

            // Assert — La comisión DEBE estar calculada sobre 5,500,000 (no sobre 5,000,000)
            var expectedBase = 5_500_000m;
            var expectedAmount = Math.Round(expectedBase * CommissionConstants.DefaultRate / 100m, 2);

            _commissionRepoMock.Verify(r => r.AddAsync(It.Is<Commission>(c =>
                c.SalePrice == expectedBase &&
                c.Amount == expectedAmount
            )), Times.Once, "La comisión debe reflejar el precio de contraoferta aceptada, no el monto original");
        }

        /// <summary>
        /// Tarea 1.1 — Caso base: cuando NO hay contraoferta, la comisión usa MontoOfertado normalmente.
        /// </summary>
        [Fact]
        public async Task CreateForAcceptedOfferAsync_DebeUsarMontoOfertado_CuandoNoExisteContraoferta()
        {
            // Arrange
            var offer = new Offer
            {
                Id = 100,
                PropertyId = 8,
                MontoOfertado = 3_000_000m,
                CounterOfferAmount = null // Sin contraoferta — flujo normal
            };
            var property = new Property { Id = 8, AgentId = "agent-B" };

            _offerRepoMock.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(offer);
            _propertyRepoMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(property);
            _commissionRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Commission>());
            _subscriptionServiceMock
                .Setup(s => s.GetCurrentSubscriptionByAgentIdAsync("agent-B"))
                .ReturnsAsync((AgentSubscriptionViewModel?)null);
            _commissionRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Commission>()))
                .ReturnsAsync((Commission c) => c);

            // Act
            await _sut.CreateForAcceptedOfferAsync(100);

            // Assert — Sin contraoferta, debe usar MontoOfertado
            var expectedAmount = Math.Round(3_000_000m * CommissionConstants.DefaultRate / 100m, 2);
            _commissionRepoMock.Verify(r => r.AddAsync(It.Is<Commission>(c =>
                c.SalePrice == 3_000_000m &&
                c.Amount == expectedAmount
            )), Times.Once, "Sin contraoferta activa, la comisión debe calcularse sobre MontoOfertado");
        }
    }
}