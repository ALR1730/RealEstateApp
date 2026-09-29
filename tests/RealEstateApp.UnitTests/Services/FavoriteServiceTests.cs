using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class FavoriteServiceTests
    {
        private readonly Mock<IFavoriteRepository> _favoriteRepoMock;
        private readonly IMapper _mapper;
        private readonly FavoriteService _sut;

        public FavoriteServiceTests()
        {
            _favoriteRepoMock = new Mock<IFavoriteRepository>();
            _mapper = AutoMapperTestFactory.CreateMapper();
            _sut = new FavoriteService(_favoriteRepoMock.Object, _mapper);
        }

        [Fact]
        public async Task AddFavorite_DebeAgregarFavorito_CuandoNoExistePreviamente()
        {
            // Arrange
            _favoriteRepoMock.Setup(r => r.GetByClienteAndPropertyAsync("client-1", 5))
                .ReturnsAsync((Favorite?)null);

            _favoriteRepoMock.Setup(r => r.AddAsync(It.IsAny<Favorite>()))
                .ReturnsAsync((Favorite f) => { f.Id = 1; return f; });

            // Act
            await _sut.AddFavorite("client-1", 5);

            // Assert
            _favoriteRepoMock.Verify(r => r.AddAsync(It.Is<Favorite>(f => f.ClienteId == "client-1" && f.PropertyId == 5)), Times.Once);
        }

        [Fact]
        public async Task AddFavorite_NoDebeDuplicar_CuandoYaExisteFavorito()
        {
            // Arrange
            var existing = new Favorite { Id = 1, ClienteId = "client-1", PropertyId = 5 };
            _favoriteRepoMock.Setup(r => r.GetByClienteAndPropertyAsync("client-1", 5))
                .ReturnsAsync(existing);

            // Act
            await _sut.AddFavorite("client-1", 5);

            // Assert
            _favoriteRepoMock.Verify(r => r.AddAsync(It.IsAny<Favorite>()), Times.Never);
        }

        [Fact]
        public async Task RemoveFavorite_DebeEliminar_CuandoExisteFavorito()
        {
            // Arrange
            var existing = new Favorite { Id = 1, ClienteId = "client-1", PropertyId = 5 };
            _favoriteRepoMock.Setup(r => r.GetByClienteAndPropertyAsync("client-1", 5))
                .ReturnsAsync(existing);

            // Act
            await _sut.RemoveFavorite("client-1", 5);

            // Assert
            _favoriteRepoMock.Verify(r => r.DeleteAsync(existing), Times.Once);
        }

        [Fact]
        public async Task IsFavorite_DebeRetornarTrue_CuandoExiste()
        {
            // Arrange
            var existing = new Favorite { Id = 1, ClienteId = "client-1", PropertyId = 5 };
            _favoriteRepoMock.Setup(r => r.GetByClienteAndPropertyAsync("client-1", 5))
                .ReturnsAsync(existing);

            // Act
            var result = await _sut.IsFavorite("client-1", 5);

            // Assert
            result.Should().BeTrue();
        }
    }
}
