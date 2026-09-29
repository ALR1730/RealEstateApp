using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class ProvinceServiceTests
    {
        private readonly Mock<IProvinceRepository> _provinceRepoMock;
        private readonly Mock<IMunicipalityRepository> _municipalityRepoMock;
        private readonly ProvinceService _sut;

        public ProvinceServiceTests()
        {
            _provinceRepoMock = new Mock<IProvinceRepository>();
            _municipalityRepoMock = new Mock<IMunicipalityRepository>();
            _sut = new ProvinceService(_provinceRepoMock.Object, _municipalityRepoMock.Object);
        }

        [Fact]
        public async Task GetAllWithMunicipalitiesAsync_DebeRetornarProvinciasConMunicipios()
        {
            // Arrange
            var provinces = new List<Province>
            {
                new Province
                {
                    Id = 1,
                    Name = "Santo Domingo",
                    Municipalities = new List<Municipality>
                    {
                        new Municipality { Id = 10, Name = "Santo Domingo Este", ProvinceId = 1 }
                    }
                }
            };

            _provinceRepoMock.Setup(r => r.GetAllWithMunicipalitiesAsync())
                .ReturnsAsync(provinces);

            // Act
            var result = await _sut.GetAllWithMunicipalitiesAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Santo Domingo");
            result[0].Municipalities.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetMunicipalitiesByProvinceIdAsync_DebeFiltrarPorIdDeProvincia()
        {
            // Arrange
            var municipalities = new List<Municipality>
            {
                new Municipality { Id = 20, Name = "Santiago", ProvinceId = 2 }
            };

            _municipalityRepoMock.Setup(r => r.GetByProvinceIdAsync(2))
                .ReturnsAsync(municipalities);

            // Act
            var result = await _sut.GetMunicipalitiesByProvinceIdAsync(2);

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Santiago");
        }
    }
}
