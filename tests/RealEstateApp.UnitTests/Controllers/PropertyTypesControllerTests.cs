using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.PropertyType;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class PropertyTypesControllerTests
    {
        private readonly Mock<IPropertyTypeService> _serviceMock;
        private readonly PropertyTypesController _controller;

        public PropertyTypesControllerTests()
        {
            _serviceMock = new Mock<IPropertyTypeService>();
            IMapper mapper = AutoMapperTestFactory.CreateMapper();
            _controller = new PropertyTypesController(_serviceMock.Object, mapper);
        }

        [Fact]
        public async Task Post_ValidViewModel_ReturnsOkWithCreatedDto()
        {
            // Arrange
            _serviceMock.Setup(s => s.Add(It.IsAny<SavePropertyTypeViewModel>()))
                .ReturnsAsync(new SavePropertyTypeViewModel
                {
                    Id = 7,
                    Name = "Casa Unifamiliar",
                    Description = "Vivienda independiente"
                });

            var vm = new SavePropertyTypeViewModel
            {
                Name = "Apto",
                Description = "Departamento en edificio"
            };

            // Act
            var result = await _controller.PostAsync(vm);

            // Assert: el POST debe devolver Ok (200) y no un CreatedAtActionResult,
            // que rompía con "No route matches the supplied values" (bug 500 en catálogos).
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            var dto = okResult!.Value?.GetType().GetProperty("Id")?.GetValue(okResult!.Value, null);
            dto.Should().Be(7);
        }

        [Fact]
        public async Task Post_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "El nombre es requerido");

            var vm = new SavePropertyTypeViewModel { Name = "", Description = "" };

            // Act
            var result = await _controller.PostAsync(vm);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _serviceMock.Verify(s => s.Add(It.IsAny<SavePropertyTypeViewModel>()), Times.Never);
        }
    }
}