using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Presentation.WebApi.Filters;
using Xunit;

namespace RealEstateApp.UnitTests.Filters
{
    public class ApiGlobalExceptionFilterTests
    {
        private readonly ApiGlobalExceptionFilter _sut = new();

        private static ExceptionContext CreateContext(Exception exception)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor(),
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());

            return new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = exception
            };
        }

        [Fact]
        public async Task OnExceptionAsync_ReturnsBadRequest_WhenValidationException()
        {
            // Arrange
            var ctx = CreateContext(new ValidationException("Mensaje de validación"));

            // Act
            await _sut.OnExceptionAsync(ctx);

            // Assert
            ctx.ExceptionHandled.Should().BeTrue();
            ctx.Result.Should().BeOfType<BadRequestObjectResult>();
            var result = (BadRequestObjectResult)ctx.Result!;
            result.StatusCode.Should().Be(400);
            var body = result.Value!;
            body.GetType().GetProperty("hasError")!.GetValue(body).Should().Be(true);
            body.GetType().GetProperty("error")!.GetValue(body).Should().Be("Mensaje de validación");
        }

        [Fact]
        public async Task OnExceptionAsync_ReturnsNotFound_WhenNotFoundException()
        {
            // Arrange
            var ctx = CreateContext(new NotFoundException("Propiedad", 42));

            // Act
            await _sut.OnExceptionAsync(ctx);

            // Assert
            ctx.ExceptionHandled.Should().BeTrue();
            ctx.Result.Should().BeOfType<NotFoundObjectResult>();
            ((NotFoundObjectResult)ctx.Result!).StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task OnExceptionAsync_NotHandled_WhenUnknownException()
        {
            // Arrange
            var ctx = CreateContext(new InvalidOperationException("boom"));

            // Act
            await _sut.OnExceptionAsync(ctx);

            // Assert
            ctx.ExceptionHandled.Should().BeFalse();
            ctx.Result.Should().BeNull();
        }
    }
}