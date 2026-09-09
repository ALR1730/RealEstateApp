using AutoMapper;
using FluentAssertions;
using RealEstateApp.Core.Application.ViewModels.LeadPipeline;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Mappings
{
    public class GeneralProfileTests
    {
        private readonly IMapper _mapper;

        public GeneralProfileTests()
        {
            _mapper = AutoMapperTestFactory.CreateMapper();
        }

        [Fact]
        public void Map_LeadPipelineConPropiedad_RetornaDtoCompleto()
        {
            var lead = new LeadPipeline
            {
                Id = 7,
                LeadName = "Ana Ferreira",
                LeadEmail = "ana.f@mail.com",
                LeadPhone = "809-555-0101",
                Notes = "Busca apartamento",
                AgentId = "agent-1",
                Stage = PipelineStage.NewLead,
                Priority = "Alta",
                Source = "Página Web",
                EstimatedBudget = 4500000,
                BudgetCurrency = "DOP",
                PropertyId = 1,
                Property = new Property { Name = "Apartamento Moderno Bella Vista" },
                LastContactDate = new System.DateTime(2026, 9, 1),
                NextFollowUpDate = new System.DateTime(2026, 9, 15),
                SortOrder = 3,
                Created = new System.DateTime(2026, 9, 1)
            };

            var dto = _mapper.Map<LeadPipelineDto>(lead);

            dto.Id.Should().Be(7);
            dto.LeadName.Should().Be("Ana Ferreira");
            dto.LeadEmail.Should().Be("ana.f@mail.com");
            dto.LeadPhone.Should().Be("809-555-0101");
            dto.Notes.Should().Be("Busca apartamento");
            dto.AgentId.Should().Be("agent-1");
            dto.Stage.Should().Be(PipelineStage.NewLead);
            dto.Priority.Should().Be("Alta");
            dto.Source.Should().Be("Página Web");
            dto.EstimatedBudget.Should().Be(4500000);
            dto.BudgetCurrency.Should().Be("DOP");
            dto.PropertyId.Should().Be(1);
            dto.PropertyName.Should().Be("Apartamento Moderno Bella Vista");
            dto.LastContactDate.Should().Be(lead.LastContactDate);
            dto.NextFollowUpDate.Should().Be(lead.NextFollowUpDate);
            dto.SortOrder.Should().Be(3);
            dto.Created.Should().Be(lead.Created);
        }

        [Fact]
        public void Map_LeadPipelineSinPropiedad_RetornaPropertyNameNull()
        {
            var lead = new LeadPipeline { Id = 1, LeadName = "Luis", AgentId = "agent-1" };

            var dto = _mapper.Map<LeadPipelineDto>(lead);

            dto.Id.Should().Be(1);
            dto.LeadName.Should().Be("Luis");
            dto.PropertyName.Should().BeNull();
        }
    }
}