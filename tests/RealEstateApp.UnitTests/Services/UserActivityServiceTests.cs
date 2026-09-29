using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.UserActivity;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class UserActivityServiceTests
    {
        private readonly Mock<IUserActivityRepository> _activityRepoMock;
        private readonly IMapper _mapper;
        private readonly UserActivityService _sut;

        public UserActivityServiceTests()
        {
            _activityRepoMock = new Mock<IUserActivityRepository>();
            _mapper = AutoMapperTestFactory.CreateMapper();
            _sut = new UserActivityService(_activityRepoMock.Object, _mapper);
        }

        [Fact]
        public async Task LogActivityAsync_DebeRegistrarActividad_CuandoUserIdEsValido()
        {
            // Arrange
            _activityRepoMock.Setup(r => r.AddAsync(It.IsAny<UserActivity>()))
                .ReturnsAsync((UserActivity a) => { a.Id = 1; return a; });

            // Act
            await _sut.LogActivityAsync("user-1", "LOGIN", "Inicio de sesión exitoso");

            // Assert
            _activityRepoMock.Verify(r => r.AddAsync(It.Is<UserActivity>(a =>
                a.UserId == "user-1" &&
                a.Action == "LOGIN" &&
                a.Description == "Inicio de sesión exitoso"
            )), Times.Once);
        }

        [Fact]
        public async Task LogActivityAsync_NoDebeRegistrar_CuandoUserIdEsVacio()
        {
            // Act
            await _sut.LogActivityAsync(string.Empty, "LOGIN", "Inicio");

            // Assert
            _activityRepoMock.Verify(r => r.AddAsync(It.IsAny<UserActivity>()), Times.Never);
        }

        [Fact]
        public async Task GetRecentActivitiesAsync_DebeRetornarMapeoDeActividades()
        {
            // Arrange
            var list = new List<UserActivity>
            {
                new UserActivity { Id = 1, UserId = "user-1", Action = "VIEW", Description = "Vio propiedad #5" }
            };

            _activityRepoMock.Setup(r => r.GetByUserIdAsync("user-1", 10))
                .ReturnsAsync(list);

            // Act
            var result = await _sut.GetRecentActivitiesAsync("user-1", 10);

            // Assert
            result.Should().HaveCount(1);
            result[0].Action.Should().Be("VIEW");
        }
    }
}
