using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.LeadPipeline;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class LeadPipelineServiceTests
    {
        private readonly Mock<ILeadPipelineRepository> _leadRepoMock;
        private readonly Mock<IPropertyRepository> _propertyRepoMock;
        private readonly IMapper _mapper;
        private readonly LeadPipelineService _sut;

        public LeadPipelineServiceTests()
        {
            _leadRepoMock = new Mock<ILeadPipelineRepository>();
            _propertyRepoMock = new Mock<IPropertyRepository>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _sut = new LeadPipelineService(
                _leadRepoMock.Object,
                _propertyRepoMock.Object,
                _mapper);
        }

        [Fact]
        public async Task CreateAsync_DebeCrearLeadEnEtapaInicial_ConOrdenSecuencial()
        {
            // Arrange
            var existingLeads = new List<LeadPipeline>
            {
                new LeadPipeline { Id = 1, AgentId = "agent-1", SortOrder = 2 }
            };

            _leadRepoMock.Setup(r => r.GetByAgentIdAsync("agent-1"))
                .ReturnsAsync(existingLeads);

            _leadRepoMock.Setup(r => r.AddAsync(It.IsAny<LeadPipeline>()))
                .ReturnsAsync((LeadPipeline l) => { l.Id = 10; return l; });

            var request = new CreateLeadRequest
            {
                LeadName = "Carlos Perez",
                LeadEmail = "carlos@example.com",
                LeadPhone = "809-555-0101",
                Priority = "High",
                EstimatedBudget = 150000m
            };

            // Act
            var result = await _sut.CreateAsync(request, "agent-1");

            // Assert
            result.Should().NotBeNull();
            _leadRepoMock.Verify(r => r.AddAsync(It.Is<LeadPipeline>(l =>
                l.AgentId == "agent-1" &&
                l.LeadName == "Carlos Perez" &&
                l.Stage == PipelineStage.NewLead &&
                l.SortOrder == 3
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateStageAsync_DebeModificarEtapaYFechaDeCierre_CuandoEsGanada()
        {
            // Arrange
            var lead = new LeadPipeline
            {
                Id = 5,
                AgentId = "agent-1",
                Stage = PipelineStage.Negotiation
            };

            _leadRepoMock.Setup(r => r.GetByIdWithPropertyAsync(5))
                .ReturnsAsync(lead);

            _leadRepoMock.Setup(r => r.UpdateAsync(It.IsAny<LeadPipeline>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateStageAsync(5, PipelineStage.ClosedWon);

            // Assert
            result.Should().NotBeNull();
            lead.Stage.Should().Be(PipelineStage.ClosedWon);
            lead.ClosedAt.Should().NotBeNull();
            _leadRepoMock.Verify(r => r.UpdateAsync(lead), Times.Once);
        }

        [Fact]
        public async Task UpdateStageAsync_DebeLanzarNotFoundException_CuandoLeadNoExiste()
        {
            // Arrange
            _leadRepoMock.Setup(r => r.GetByIdWithPropertyAsync(999))
                .ReturnsAsync((LeadPipeline?)null);

            // Act
            Func<Task> act = async () => await _sut.UpdateStageAsync(999, PipelineStage.Contacted);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetStatsAsync_DebeCalcularTasasDeConversionYValoresTotales()
        {
            // Arrange
            var leads = new List<LeadPipeline>
            {
                new LeadPipeline { Id = 1, AgentId = "agent-1", Stage = PipelineStage.ClosedWon, EstimatedBudget = 100000m },
                new LeadPipeline { Id = 2, AgentId = "agent-1", Stage = PipelineStage.ClosedLost, EstimatedBudget = 50000m },
                new LeadPipeline { Id = 3, AgentId = "agent-1", Stage = PipelineStage.NewLead, EstimatedBudget = 80000m },
                new LeadPipeline { Id = 4, AgentId = "agent-1", Stage = PipelineStage.Negotiation, EstimatedBudget = 120000m }
            };

            _leadRepoMock.Setup(r => r.GetByAgentIdAsync("agent-1"))
                .ReturnsAsync(leads);

            // Act
            var stats = await _sut.GetStatsAsync("agent-1");

            // Assert
            stats.TotalLeads.Should().Be(4);
            stats.WonLeads.Should().Be(1);
            stats.LostLeads.Should().Be(1);
            stats.ActiveLeads.Should().Be(2);
            stats.WonValue.Should().Be(100000m);
            stats.PipelineValue.Should().Be(200000m);
            stats.ConversionRate.Should().Be(25.0m);
        }
    }
}
