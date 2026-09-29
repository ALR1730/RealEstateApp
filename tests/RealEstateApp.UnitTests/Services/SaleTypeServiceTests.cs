using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.SaleType;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class SaleTypeServiceTests
    {
        private readonly Mock<IGenericRepository<SaleType>> _repoMock;
        private readonly IMapper _mapper;
        private readonly SaleTypeService _sut;

        public SaleTypeServiceTests()
        {
            _repoMock = new Mock<IGenericRepository<SaleType>>();
            _mapper = AutoMapperTestFactory.CreateMapper();
            _sut = new SaleTypeService(_repoMock.Object, _mapper);
        }

        [Fact]
        public async Task GetAllViewModel_DebeRetornarTodosLosTiposDeVenta()
        {
            // Arrange
            var list = new List<SaleType>
            {
                new SaleType { Id = 1, Name = "Venta", Description = "Transmisión definitiva" },
                new SaleType { Id = 2, Name = "Alquiler", Description = "Renta mensual" }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _sut.GetAllViewModel();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Venta");
            result[1].Name.Should().Be("Alquiler");
        }

        [Fact]
        public async Task Add_DebeGuardarYRetornarViewModel()
        {
            // Arrange
            var vm = new SaveSaleTypeViewModel
            {
                Name = "Alquiler Vacacional",
                Description = "Estadías cortas"
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<SaleType>()))
                .ReturnsAsync((SaleType st) => { st.Id = 10; return st; });

            // Act
            var result = await _sut.Add(vm);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(10);
            _repoMock.Verify(r => r.AddAsync(It.Is<SaleType>(st => st.Name == "Alquiler Vacacional")), Times.Once);
        }
    }
}
