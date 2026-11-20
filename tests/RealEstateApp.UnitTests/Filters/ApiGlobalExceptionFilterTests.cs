using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
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
            httpContext.Request.Path = "/api/v1/test";
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
            var result = ctx.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            result.StatusCode.Should().Be(400);

            var problem = result.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(400);
            problem.Detail.Should().Be("Mensaje de validación");
            problem.Title.Should().NotBeNullOrWhiteSpace();
            problem.Type.Should().NotBeNullOrWhiteSpace();
            problem.Instance.Should().Be("/api/v1/test");
            problem.Extensions["hasError"].Should().Be(true);
            problem.Extensions["error"].Should().Be("Mensaje de validación");
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
            var result = ctx.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            result.StatusCode.Should().Be(404);

            var problem = result.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(404);
            problem.Detail.Should().Contain("Propiedad");
            problem.Extensions["hasError"].Should().Be(true);
        }

        [Fact]
        public async Task OnExceptionAsync_ReturnsUnprocessableEntity_WhenDomainException()
        {
            // Arrange
            var ctx = CreateContext(new DomainException("Transición inválida de dominio"));

            // Act
            await _sut.OnExceptionAsync(ctx);

            // Assert
            ctx.ExceptionHandled.Should().BeTrue();
            var result = ctx.Result.Should().BeOfType<UnprocessableEntityObjectResult>().Subject;
            result.StatusCode.Should().Be(422);

            var problem = result.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(422);
            problem.Detail.Should().Be("Transición inválida de dominio");
            problem.Extensions["hasError"].Should().Be(true);
            problem.Extensions["error"].Should().Be("Transición inválida de dominio");
        }

        [Fact]
        public async Task OnExceptionAsync_ReturnsConflict_WhenDbUpdateConcurrencyException()
        {
            // Arrange
            var ctx = CreateContext(new DbUpdateConcurrencyException("Concurrencia"));

            // Act
            await _sut.OnExceptionAsync(ctx);

            // Assert
            ctx.ExceptionHandled.Should().BeTrue();
            var result = ctx.Result.Should().BeOfType<ConflictObjectResult>().Subject;
            result.StatusCode.Should().Be(409);

            var problem = result.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(409);
            problem.Extensions["hasError"].Should().Be(true);
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
