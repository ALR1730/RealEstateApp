using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.DTOs.Common;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using RealEstateApp.UnitTests.Common;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class PropertiesControllerPaginationTests
    {
        private readonly Mock<IPropertyService> _propertyServiceMock;
        private readonly PropertiesController _controller;

        public PropertiesControllerPaginationTests()
        {
            _propertyServiceMock = new Mock<IPropertyService>();
            _controller = new PropertiesController(
                _propertyServiceMock.Object,
                AutoMapperTestFactory.CreateMapper());
        }

        [Fact]
        public async Task GetAsync_DebeRetornarPagedResultLimitadoA50_CuandoQuerySolicitaPageSize200()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var sampleItems = new List<PropertyViewModel>
            {
                new() { Id = 1, Code = "APT001", Name = "Apto 1", Status = PropertyStatus.Available }
            };

            _propertyServiceMock
                .Setup(s => s.GetWithFiltersPagedAsync(It.IsAny<PropertyFilterViewModel>(), false))
                .ReturnsAsync((PropertyFilterViewModel f, bool _) =>
                {
                    var (page, pageSize) = PagedResult<PropertyViewModel>.Normalize(f.Page ?? f.PageNumber, f.PageSize);
                    return PagedResult<PropertyViewModel>.Create(sampleItems, page, pageSize, 120);
                });

            var filter = new PropertyFilterViewModel { Page = 1, PageSize = 200 };

            // Act
            var actionResult = await _controller.GetAsync(filter);

            // Assert
            var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
            var payload = ok.Value.Should().BeOfType<PagedResult<PropertyDto>>().Subject;
            payload.Page.Should().Be(1);
            payload.PageSize.Should().Be(PagedResult<PropertyDto>.MaxPageSize);
            payload.TotalCount.Should().Be(120);
            payload.TotalPages.Should().Be(3);
            payload.Items.Should().HaveCount(1);
            _propertyServiceMock.Verify(s => s.GetWithFiltersPagedAsync(filter, false), Times.Once);
        }

        [Fact]
        public async Task GetAsync_DebePasarIncludeSoldFalseParaPublico_YIncludeSoldTrueParaAdmin()
        {
            // Arrange
            var emptyPage = PagedResult<PropertyViewModel>.Create(new List<PropertyViewModel>(), 1, 20, 0);
            _propertyServiceMock
                .Setup(s => s.GetWithFiltersPagedAsync(It.IsAny<PropertyFilterViewModel>(), It.IsAny<bool>()))
                .ReturnsAsync(emptyPage);

            // Act 1: Usuario anónimo/público
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            await _controller.GetAsync(new PropertyFilterViewModel());

            // Act 2: Usuario Admin
            var adminIdentity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                    new Claim(ClaimTypes.Role, "Admin")
                },
                authenticationType: "Test",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(adminIdentity)
                }
            };
            await _controller.GetAsync(new PropertyFilterViewModel());

            // Assert
            _propertyServiceMock.Verify(
                s => s.GetWithFiltersPagedAsync(It.IsAny<PropertyFilterViewModel>(), false),
                Times.Once);
            _propertyServiceMock.Verify(
                s => s.GetWithFiltersPagedAsync(It.IsAny<PropertyFilterViewModel>(), true),
                Times.Once);
        }
    }
}
