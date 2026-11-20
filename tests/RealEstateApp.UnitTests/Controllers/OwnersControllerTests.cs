using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using RealEstateApp.Presentation.WebApi.Models.Requests;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class OwnersControllerTests
    {
        private const string OwnerId = "owner-123";

        private readonly Mock<IPropertyService> _propertyServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly OwnersController _controller;

        public OwnersControllerTests()
        {
            _propertyServiceMock = new Mock<IPropertyService>();
            _mapperMock = new Mock<IMapper>();

            _controller = new OwnersController(
                _propertyServiceMock.Object,
                _mapperMock.Object);
        }

        private static ClaimsPrincipal CreateOwnerPrincipal()
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, OwnerId) },
                authenticationType: "Test",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task GetProperty_OwnerOwnsProperty_ReturnsOkWithDto()
        {
            // Arrange
            var vm = new PropertyViewModel { Id = 5, AgentId = OwnerId, Name = "Casa Test", Price = 300000m };
            var dto = new PropertyDto { Id = 5, Name = "Casa Test" };

            _propertyServiceMock.Setup(s => s.GetByIdViewModel(5)).ReturnsAsync(vm);
            _mapperMock.Setup(m => m.Map<PropertyDto>(vm)).Returns(dto);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.GetProperty(5);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(dto);
        }

        [Fact]
        public async Task GetProperty_NotOwner_ReturnsNotFound()
        {
            // Arrange
            var vm = new PropertyViewModel { Id = 5, AgentId = "another-owner", Name = "Otra Propiedad" };
            _propertyServiceMock.Setup(s => s.GetByIdViewModel(5)).ReturnsAsync(vm);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.GetProperty(5);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetProperty_NotFound_ReturnsNotFound()
        {
            // Arrange
            _propertyServiceMock.Setup(s => s.GetByIdViewModel(99)).ReturnsAsync((PropertyViewModel?)null);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.GetProperty(99);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateProperty_OwnerOwnsProperty_ReturnsOk()
        {
            // Arrange
            var request = new SavePropertyFormRequest { Id = 7, Name = "Casa Actualizada", Price = 350000m };
            var existing = new PropertyViewModel { Id = 7, AgentId = OwnerId, Name = "Casa" };

            _propertyServiceMock.Setup(s => s.GetByIdViewModel(7)).ReturnsAsync(existing);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.UpdateProperty(7, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _propertyServiceMock.Verify(
                s => s.Update(
                    It.Is<SavePropertyViewModel>(v => v.Id == 7 && v.AgentId == OwnerId && v.Name == "Casa Actualizada" && v.Price == 350000m),
                    7),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProperty_NotOwner_ReturnsNotFound()
        {
            // Arrange
            var request = new SavePropertyFormRequest { Id = 7, Name = "Casa" };
            var existing = new PropertyViewModel { Id = 7, AgentId = "another-owner", Name = "Otra" };

            _propertyServiceMock.Setup(s => s.GetByIdViewModel(7)).ReturnsAsync(existing);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.UpdateProperty(7, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            _propertyServiceMock.Verify(s => s.Update(It.IsAny<SavePropertyViewModel>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProperty_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var request = new SavePropertyFormRequest { Id = 8, Name = "Casa" };
            var existing = new PropertyViewModel { Id = 7, AgentId = OwnerId, Name = "Otra" };

            _propertyServiceMock.Setup(s => s.GetByIdViewModel(7)).ReturnsAsync(existing);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.UpdateProperty(7, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _propertyServiceMock.Verify(s => s.Update(It.IsAny<SavePropertyViewModel>(), It.IsAny<int>()), Times.Never);
        }
    }
}
