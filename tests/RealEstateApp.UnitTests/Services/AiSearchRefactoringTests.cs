using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RealEstateApp.Core.Application.DTOs.AiSearch;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.Services.AiSearch;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;
using ImprovementVM = RealEstateApp.Core.Application.ViewModels.Improvement.ImprovementViewModel;
using PropertyTypeVM = RealEstateApp.Core.Application.ViewModels.PropertyType.PropertyTypeViewModel;
using SaleTypeVM = RealEstateApp.Core.Application.ViewModels.SaleType.SaleTypeViewModel;

namespace RealEstateApp.UnitTests.Services
{
    public class AiSearchRefactoringTests
    {
        private readonly Mock<IPropertyService> _propertyServiceMock;
        private readonly Mock<IPropertyTypeService> _propertyTypeServiceMock;
        private readonly Mock<ISaleTypeService> _saleTypeServiceMock;
        private readonly Mock<IImprovementService> _improvementServiceMock;
        private readonly Mock<IProvinceService> _provinceServiceMock;
        private readonly IMapper _mapper;

        public AiSearchRefactoringTests()
        {
            _propertyServiceMock = new Mock<IPropertyService>();
            _propertyTypeServiceMock = new Mock<IPropertyTypeService>();
            _saleTypeServiceMock = new Mock<ISaleTypeService>();
            _improvementServiceMock = new Mock<IImprovementService>();
            _provinceServiceMock = new Mock<IProvinceService>();
            _mapper = AutoMapperTestFactory.CreateMapper();

            _propertyTypeServiceMock.Setup(s => s.GetAllViewModel()).ReturnsAsync(new List<PropertyTypeVM>
            {
                new() { Id = 1, Name = "Apartamento" },
                new() { Id = 2, Name = "Casa" }
            });

            _saleTypeServiceMock.Setup(s => s.GetAllViewModel()).ReturnsAsync(new List<SaleTypeVM>
            {
                new() { Id = 1, Name = "Venta Directa" },
                new() { Id = 2, Name = "Alquiler" }
            });

            _improvementServiceMock.Setup(s => s.GetAllViewModel()).ReturnsAsync(new List<ImprovementVM>
            {
                new() { Id = 10, Name = "Piscina" },
                new() { Id = 11, Name = "Balcón" }
            });

            _provinceServiceMock.Setup(s => s.GetAllWithMunicipalitiesAsync()).ReturnsAsync(new List<Province>
            {
                new() { Id = 1, Name = "Distrito Nacional" },
                new() { Id = 2, Name = "Santiago" }
            });
        }

        [Fact]
        public async Task InterpretQueryAsync_DebeCachearCatalogosEnMemoryCachePor5Minutos_EvitandoConsultasRepetidas()
        {
            // Arrange
            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var sut = new AiSearchService(
                _propertyServiceMock.Object,
                _propertyTypeServiceMock.Object,
                _saleTypeServiceMock.Object,
                _improvementServiceMock.Object,
                _provinceServiceMock.Object,
                _mapper,
                memoryCache);

            // Act
            await sut.InterpretQueryAsync("Apartamento en Bella Vista");
            await sut.InterpretQueryAsync("Casa en Santiago con piscina");
            await sut.InterpretQueryAsync("Apartamento en alquiler con balcón");

            // Assert
            _propertyTypeServiceMock.Verify(s => s.GetAllViewModel(), Times.Once);
            _saleTypeServiceMock.Verify(s => s.GetAllViewModel(), Times.Once);
            _improvementServiceMock.Verify(s => s.GetAllViewModel(), Times.Once);
            _provinceServiceMock.Verify(s => s.GetAllWithMunicipalitiesAsync(), Times.Once);
        }

        [Fact]
        public async Task InterpretQueryAsync_DebeCompletarRapidoSinReDoS_AnteEntradaAdversarialLarga()
        {
            // Arrange
            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var sut = new AiSearchService(
                _propertyServiceMock.Object,
                _propertyTypeServiceMock.Object,
                _saleTypeServiceMock.Object,
                _improvementServiceMock.Object,
                _provinceServiceMock.Object,
                _mapper,
                memoryCache);

            var adversarial = "entre " + new string('9', 10000) + new string(' ', 5000) + new string('a', 15000);
            var sw = Stopwatch.StartNew();

            // Act
            var result = await sut.InterpretQueryAsync(adversarial);
            sw.Stop();

            // Assert
            result.Should().NotBeNull();
            sw.ElapsedMilliseconds.Should().BeLessThan(2000);
            AiSearchTextHelper.RegexTimeout.TotalMilliseconds.Should().Be(200);
        }

        [Fact]
        public void Extractors_DebenExtraerEntidadesDeFormaModularYDesacoplada()
        {
            // Arrange
            var catalog = new AiSearchCatalogSnapshot(
                new List<PropertyTypeVM> { new() { Id = 1, Name = "Apartamento" } },
                new List<SaleTypeVM> { new() { Id = 1, Name = "Venta" }, new() { Id = 2, Name = "Alquiler" } },
                new List<ImprovementVM> { new() { Id = 10, Name = "Piscina" } },
                new List<Province> { new() { Id = 2, Name = "Santiago" } });

            var query = AiSearchTextHelper.NormalizeText(
                "Apartamento en alquiler en Santiago de 3 habitaciones 2 baños entre 40 mil y 60 mil con piscina y tour virtual");
            var context = new AiExtractionContext(query, catalog);

            IAiQueryEntityExtractor[] extractors =
            {
                new PropertyTypeExtractor(),
                new SaleTypeExtractor(),
                new RoomsAndBathroomsExtractor(),
                new PriceRangeExtractor(),
                new LocationExtractor(),
                new ImprovementExtractor(),
                new SpecialCriteriaExtractor()
            };

            // Act
            foreach (var extractor in extractors)
            {
                extractor.Extract(context);
            }

            // Assert
            context.Filter.PropertyTypeId.Should().Be(1);
            context.Filter.SaleTypeId.Should().Be(2);
            context.Filter.ProvinceId.Should().Be(2);
            context.Filter.MinRooms.Should().Be(3);
            context.Filter.MinBathrooms.Should().Be(2);
            context.Filter.MinPrice.Should().Be(40_000m);
            context.Filter.MaxPrice.Should().Be(60_000m);
            context.Filter.ImprovementIds.Should().Contain(10);
            context.Filter.OnlyWithVirtualTour.Should().BeTrue();
            context.MatchedSignals.Should().BeGreaterThanOrEqualTo(7);
        }
    }
}
