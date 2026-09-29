using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Improvement;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class ImprovementServiceTests
    {
        private readonly Mock<IGenericRepository<Improvement>> _repoMock;
        private readonly IMapper _mapper;
        private readonly ImprovementService _sut;

        public ImprovementServiceTests()
        {
            _repoMock = new Mock<IGenericRepository<Improvement>>();
            _mapper = AutoMapperTestFactory.CreateMapper();
            _sut = new ImprovementService(_repoMock.Object, _mapper);
        }

        [Fact]
        public async Task GetAllViewModel_DebeRetornarListaDeMejoras()
        {
            // Arrange
            var improvements = new List<Improvement>
            {
                new Improvement { Id = 1, Name = "Piscina", Description = "Piscina privada" },
                new Improvement { Id = 2, Name = "Ascensor", Description = "Ascensor moderno" }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(improvements);

            // Act
            var result = await _sut.GetAllViewModel();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Piscina");
            result[1].Name.Should().Be("Ascensor");
        }

        [Fact]
        public async Task Add_DebeGuardarYRetornarSaveViewModel()
        {
            // Arrange
            var vm = new SaveImprovementViewModel { Name = "Planta Eléctrica", Description = "Energía continua" };
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Improvement>()))
                .ReturnsAsync((Improvement i) => { i.Id = 15; return i; });

            // Act
            var result = await _sut.Add(vm);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(15);
            _repoMock.Verify(r => r.AddAsync(It.Is<Improvement>(i => i.Name == "Planta Eléctrica")), Times.Once);
        }
    }
}
