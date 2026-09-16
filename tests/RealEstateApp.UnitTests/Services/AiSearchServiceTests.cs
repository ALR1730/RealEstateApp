using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;
using ImprovementVM = RealEstateApp.Core.Application.ViewModels.Improvement.ImprovementViewModel;
using PropertyTypeVM = RealEstateApp.Core.Application.ViewModels.PropertyType.PropertyTypeViewModel;
using SaleTypeVM = RealEstateApp.Core.Application.ViewModels.SaleType.SaleTypeViewModel;

namespace RealEstateApp.UnitTests.Services
{
    public class AiSearchServiceTests
    {
        private readonly Mock<IPropertyService> _propertyServiceMock;
        private readonly Mock<IPropertyTypeService> _propertyTypeServiceMock;
        private readonly Mock<ISaleTypeService> _saleTypeServiceMock;
        private readonly Mock<IImprovementService> _improvementServiceMock;
        private readonly Mock<IProvinceService> _provinceServiceMock;
        private readonly IMapper _mapper;
        private readonly AiSearchService _sut;

        public AiSearchServiceTests()
        {
            _propertyServiceMock = new Mock<IPropertyService>();
            _propertyTypeServiceMock = new Mock<IPropertyTypeService>();
            _saleTypeServiceMock = new Mock<ISaleTypeService>();
            _improvementServiceMock = new Mock<IImprovementService>();
            _provinceServiceMock = new Mock<IProvinceService>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            // Setup default catalog mocks
            _propertyTypeServiceMock.Setup(s => s.GetAllViewModel())
                .ReturnsAsync(new List<PropertyTypeVM>
                {
                    new() { Id = 1, Name = "Apartamento", Description = "Apartamentos" },
                    new() { Id = 2, Name = "Casa", Description = "Casas" },
                    new() { Id = 3, Name = "Villa", Description = "Villas" }
                });

            _saleTypeServiceMock.Setup(s => s.GetAllViewModel())
                .ReturnsAsync(new List<SaleTypeVM>
                {
                    new() { Id = 1, Name = "Venta", Description = "En venta" },
                    new() { Id = 2, Name = "Alquiler", Description = "En alquiler" }
                });

            _improvementServiceMock.Setup(s => s.GetAllViewModel())
                .ReturnsAsync(new List<ImprovementVM>
                {
                    new() { Id = 10, Name = "Piscina", Description = "Piscina" },
                    new() { Id = 11, Name = "Balcón", Description = "Balcón" },
                    new() { Id = 12, Name = "Gimnasio", Description = "Gimnasio" },
                    new() { Id = 13, Name = "Ascensor", Description = "Ascensor" }
                });

            _provinceServiceMock.Setup(s => s.GetAllWithMunicipalitiesAsync())
                .ReturnsAsync(new List<Province>
                {
                    new() { Id = 1, Name = "Distrito Nacional" },
                    new() { Id = 2, Name = "Santiago" },
                    new() { Id = 3, Name = "La Altagracia" }
                });

            _sut = new AiSearchService(
                _propertyServiceMock.Object,
                _propertyTypeServiceMock.Object,
                _saleTypeServiceMock.Object,
                _improvementServiceMock.Object,
                _provinceServiceMock.Object,
                _mapper
            );
        }

        [Fact]
        public async Task InterpretQueryAsync_DebeExtraerTipoYSector_CuandoSeIndicaApartamentoEnBellaVista()
        {
            // Arrange
            var query = "Apartamento en venta en Bella Vista";

            // Act
            var result = await _sut.InterpretQueryAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.ExtractedEntities.PropertyType.Should().Be("Apartamento");
            result.ExtractedEntities.SaleType.Should().Be("Venta");
            result.ExtractedEntities.Sector.Should().Be("Bella Vista");
            result.ParsedFilter.PropertyTypeId.Should().Be(1);
            result.ParsedFilter.SaleTypeId.Should().Be(1);
            result.ParsedFilter.Sector.Should().Be("Bella Vista");
            result.ConfidenceScore.Should().BeGreaterThan(0.7);
        }

        [Fact]
        public async Task InterpretQueryAsync_DebeExtraerHabitacionesYBanos_CuandoSeIndicanCantidades()
        {
            // Arrange
            var query = "Casa de 3 habitaciones y 2 baños en Santiago";

            // Act
            var result = await _sut.InterpretQueryAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.ExtractedEntities.PropertyType.Should().Be("Casa");
            result.ExtractedEntities.MinRooms.Should().Be(3);
            result.ExtractedEntities.MinBathrooms.Should().Be(2);
            result.ExtractedEntities.Province.Should().Be("Santiago");
            result.ParsedFilter.PropertyTypeId.Should().Be(2);
            result.ParsedFilter.MinRooms.Should().Be(3);
            result.ParsedFilter.MinBathrooms.Should().Be(2);
            result.ParsedFilter.ProvinceId.Should().Be(2);
        }

        [Fact]
        public async Task InterpretQueryAsync_DebeExtraerPrecioMaximoYMinimo_CuandoSeIndicaRangoOTope()
        {
            // Arrange
            var query = "Apartamento por menos de 8 millones con balcón";

            // Act
            var result = await _sut.InterpretQueryAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.ExtractedEntities.PropertyType.Should().Be("Apartamento");
            result.ExtractedEntities.MaxPrice.Should().Be(8_000_000m);
            result.ExtractedEntities.Improvements.Should().Contain("Balcón");
            result.ParsedFilter.MaxPrice.Should().Be(8_000_000m);
            result.ParsedFilter.ImprovementIds.Should().Contain(11);
        }

        [Fact]
        public async Task InterpretQueryAsync_DebeManejarConsultaVacia_RetornandoFiltroPorDefecto()
        {
            // Arrange
            var query = "";

            // Act
            var result = await _sut.InterpretQueryAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.ConfidenceScore.Should().Be(0.0);
            result.Explanation.Should().Contain("vacía");
        }

        [Fact]
        public async Task SearchByNaturalLanguageAsync_DebeRetornarPropiedadesFiltradas_CuandoExistenCoincidencias()
        {
            // Arrange
            var query = "Villa en Punta Cana con piscina";
            var mockProperties = new List<PropertyViewModel>
            {
                new()
                {
                    Id = 100,
                    Code = "VIL100",
                    Price = 15000000m,
                    Rooms = 4,
                    Bathrooms = 3,
                    PropertyTypeName = "Villa",
                    Sector = "Punta Cana",
                    Status = RealEstateApp.Core.Domain.Constants.PropertyStatus.Available
                }
            };

            _propertyServiceMock.Setup(p => p.GetAllWithFilters(It.IsAny<PropertyFilterViewModel>()))
                .ReturnsAsync(mockProperties);

            // Act
            var result = await _sut.SearchByNaturalLanguageAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.ExtractedEntities.PropertyType.Should().Be("Villa");
            result.ExtractedEntities.Sector.Should().Be("Punta Cana");
            result.ExtractedEntities.Improvements.Should().Contain("Piscina");
            result.MatchedPropertiesCount.Should().Be(1);
            result.Properties.Should().HaveCount(1);
            result.Properties[0].Code.Should().Be("VIL100");
        }
    }
}
