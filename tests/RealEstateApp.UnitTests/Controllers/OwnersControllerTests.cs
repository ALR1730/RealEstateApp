using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class OwnersControllerTests
    {
        private const string OwnerId = "owner-123";

        private readonly Mock<IPropertyService> _propertyServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly OwnersController _controller;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IdentityUser _identityUser;

        public OwnersControllerTests()
        {
            _propertyServiceMock = new Mock<IPropertyService>();
            _mapperMock = new Mock<IMapper>();

            _identityUser = new IdentityUser { Id = OwnerId, UserName = "owneruser" };
            var store = new InMemoryUserStore();
            store.Users[_identityUser.Id] = _identityUser;
            var options = new IdentityOptions();
            _userManager = new UserManager<IdentityUser>(
                store,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!)
            {
                Options = options
            };

            _controller = new OwnersController(
                _propertyServiceMock.Object,
                _userManager,
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
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
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
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
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
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
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
            var vm = new SavePropertyViewModel { Id = 7, Name = "Casa Actualizada", Price = 350000m };
            var existing = new PropertyViewModel { Id = 7, AgentId = OwnerId, Name = "Casa" };

            _propertyServiceMock.Setup(s => s.GetByIdViewModel(7)).ReturnsAsync(existing);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.UpdateProperty(7, vm);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _propertyServiceMock.Verify(s => s.Update(vm, 7), Times.Once);
        }

        [Fact]
        public async Task UpdateProperty_NotOwner_ReturnsNotFound()
        {
            // Arrange
            var vm = new SavePropertyViewModel { Id = 7, Name = "Casa" };
            var existing = new PropertyViewModel { Id = 7, AgentId = "another-owner", Name = "Otra" };

            _propertyServiceMock.Setup(s => s.GetByIdViewModel(7)).ReturnsAsync(existing);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.UpdateProperty(7, vm);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            _propertyServiceMock.Verify(s => s.Update(It.IsAny<SavePropertyViewModel>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProperty_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var vm = new SavePropertyViewModel { Id = 8, Name = "Casa" };
            var existing = new PropertyViewModel { Id = 7, AgentId = OwnerId, Name = "Otra" };

            _propertyServiceMock.Setup(s => s.GetByIdViewModel(7)).ReturnsAsync(existing);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = CreateOwnerPrincipal()
                }
            };

            // Act
            var result = await _controller.UpdateProperty(7, vm);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _propertyServiceMock.Verify(s => s.Update(It.IsAny<SavePropertyViewModel>(), It.IsAny<int>()), Times.Never);
        }
    }

    internal sealed class InMemoryUserStore : IUserStore<IdentityUser>,
        IUserPasswordStore<IdentityUser>
    {
        public Dictionary<string, IdentityUser> Users { get; } = new Dictionary<string, IdentityUser>();

        public Task<IdentityResult> CreateAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            Users[user.Id] = user;
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            Users.Remove(user.Id);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityUser?> FindByIdAsync(string userId, System.Threading.CancellationToken cancellationToken)
        {
            Users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }

        public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, System.Threading.CancellationToken cancellationToken)
        {
            var user = Users.Values.FirstOrDefault(u => (u.NormalizedUserName ?? u.UserName ?? "").Equals(normalizedUserName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(user);
        }

        public Task<IdentityResult> UpdateAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            Users[user.Id] = user;
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<string?> GetUserIdAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.Id);
        }

        public Task<string?> GetUserNameAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.UserName);
        }

        public Task SetUserNameAsync(IdentityUser user, string? userName, System.Threading.CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, System.Threading.CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(IdentityUser user, string? passwordHash, System.Threading.CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string?> GetPasswordHashAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(IdentityUser user, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public void Dispose() { }
    }
}
