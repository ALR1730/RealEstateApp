using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.PropertyType;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class PropertyTypeServiceTests
    {
        private readonly Mock<IGenericRepository<PropertyType>> _repoMock;
        private readonly IMapper _mapper;
        private readonly PropertyTypeService _sut;

        public PropertyTypeServiceTests()
        {
            _repoMock = new Mock<IGenericRepository<PropertyType>>();
            _mapper = AutoMapperTestFactory.CreateMapper();
            _sut = new PropertyTypeService(_repoMock.Object, _mapper);
        }

        [Fact]
        public async Task GetAllViewModel_DebeRetornarTodosLosTiposDePropiedad()
        {
            // Arrange
            var list = new List<PropertyType>
            {
                new PropertyType { Id = 1, Name = "Apartamento", Description = "Inmueble vertical" },
                new PropertyType { Id = 2, Name = "Villa", Description = "Inmueble con amplio terreno" }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            // Act
            var result = await _sut.GetAllViewModel();

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Apartamento");
            result[1].Name.Should().Be("Villa");
        }

        [Fact]
        public async Task Add_DebeMapearYGuardarEntidad()
        {
            // Arrange
            var vm = new SavePropertyTypeViewModel
            {
                Name = "Penthouse",
                Description = "Último piso exclusivo"
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<PropertyType>()))
                .ReturnsAsync((PropertyType pt) => { pt.Id = 5; return pt; });

            // Act
            var result = await _sut.Add(vm);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(5);
            _repoMock.Verify(r => r.AddAsync(It.Is<PropertyType>(pt => pt.Name == "Penthouse")), Times.Once);
        }

        [Fact]
        public async Task Delete_DebeEliminar_CuandoEntidadExiste()
        {
            // Arrange
            var entity = new PropertyType { Id = 3, Name = "Solar" };
            _repoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(entity);

            // Act
            await _sut.Delete(3);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(entity), Times.Once);
        }
    }
}
