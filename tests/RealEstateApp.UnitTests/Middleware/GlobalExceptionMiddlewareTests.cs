using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Presentation.WebApi.Middleware;
using Xunit;

namespace RealEstateApp.UnitTests.Middleware
{
    public class GlobalExceptionMiddlewareTests
    {
        private static DefaultHttpContext CreateHttpContext(string path = "/api/v1/offers/1")
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.Request.Path = path;
            return httpContext;
        }

        private static async Task<JsonDocument> ReadResponseJsonAsync(HttpContext httpContext)
        {
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(httpContext.Response.Body);
            var body = await reader.ReadToEndAsync();
            body.Should().NotBeNullOrWhiteSpace();
            return JsonDocument.Parse(body);
        }

        [Fact]
        public async Task InvokeAsync_DebeContinuarSinAlterarRespuesta_CuandoNoOcurreExcepcion()
        {
            // Arrange
            var httpContext = CreateHttpContext();
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var wasNextInvoked = false;

            var middleware = new GlobalExceptionMiddleware(
                next: ctx =>
                {
                    wasNextInvoked = true;
                    ctx.Response.StatusCode = StatusCodes.Status200OK;
                    return Task.CompletedTask;
                },
                logger: loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            wasNextInvoked.Should().BeTrue();
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact]
        public async Task InvokeAsync_DebeRetornar400ProblemDetails_CuandoLanzaValidationException()
        {
            // Arrange
            var httpContext = CreateHttpContext("/api/v1/offers/1");
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var middleware = new GlobalExceptionMiddleware(
                next: _ => throw new ValidationException("dato inválido"),
                logger: loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            httpContext.Response.ContentType.Should().Contain("application/problem+json");

            using var doc = await ReadResponseJsonAsync(httpContext);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(400);
            root.GetProperty("detail").GetString().Should().Be("dato inválido");
            root.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("type").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("instance").GetString().Should().Be("/api/v1/offers/1");
            root.GetProperty("hasError").GetBoolean().Should().BeTrue();
            root.GetProperty("error").GetString().Should().Be("dato inválido");
        }

        [Fact]
        public async Task InvokeAsync_DebeRetornar404ProblemDetails_CuandoLanzaNotFoundException()
        {
            // Arrange
            var httpContext = CreateHttpContext("/api/v1/offers/1");
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var middleware = new GlobalExceptionMiddleware(
                next: _ => throw new NotFoundException("no existe"),
                logger: loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            httpContext.Response.ContentType.Should().Contain("application/problem+json");

            using var doc = await ReadResponseJsonAsync(httpContext);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(404);
            root.GetProperty("detail").GetString().Should().Be("no existe");
            root.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("type").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("instance").GetString().Should().Be("/api/v1/offers/1");
            root.GetProperty("hasError").GetBoolean().Should().BeTrue();
            root.GetProperty("error").GetString().Should().Be("no existe");
        }

        [Fact]
        public async Task InvokeAsync_DebeRetornar422ProblemDetails_CuandoLanzaDomainException()
        {
            // Arrange
            var httpContext = CreateHttpContext("/api/v1/offers/1");
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var middleware = new GlobalExceptionMiddleware(
                next: _ => throw new DomainException("regla violada"),
                logger: loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
            httpContext.Response.ContentType.Should().Contain("application/problem+json");

            using var doc = await ReadResponseJsonAsync(httpContext);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(422);
            root.GetProperty("detail").GetString().Should().Be("regla violada");
            root.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("type").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("instance").GetString().Should().Be("/api/v1/offers/1");
            root.GetProperty("hasError").GetBoolean().Should().BeTrue();
            root.GetProperty("error").GetString().Should().Be("regla violada");
        }

        [Fact]
        public async Task InvokeAsync_DebeRetornar403ProblemDetails_CuandoLanzaUnauthorizedAccessException()
        {
            // Arrange
            var httpContext = CreateHttpContext("/api/v1/offers/1");
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var middleware = new GlobalExceptionMiddleware(
                next: _ => throw new UnauthorizedAccessException("prohibido"),
                logger: loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            httpContext.Response.ContentType.Should().Contain("application/problem+json");

            using var doc = await ReadResponseJsonAsync(httpContext);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(403);
            root.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("type").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("instance").GetString().Should().Be("/api/v1/offers/1");
            root.GetProperty("hasError").GetBoolean().Should().BeTrue();
            root.GetProperty("error").GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task InvokeAsync_DebeRetornar409ProblemDetails_CuandoLanzaDbUpdateConcurrencyException()
        {
            // Arrange
            var httpContext = CreateHttpContext("/api/v1/offers/1");
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var middleware = new GlobalExceptionMiddleware(
                next: _ => throw new DbUpdateConcurrencyException("conflicto"),
                logger: loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            httpContext.Response.ContentType.Should().Contain("application/problem+json");

            using var doc = await ReadResponseJsonAsync(httpContext);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(409);
            root.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("type").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("instance").GetString().Should().Be("/api/v1/offers/1");
            root.GetProperty("hasError").GetBoolean().Should().BeTrue();
            root.GetProperty("error").GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task InvokeAsync_DebeRetornar500SinExponerDetallesInternos_CuandoLanzaExcepcionNoControlada()
        {
            // Arrange
            var httpContext = CreateHttpContext("/api/v1/offers/1");
            var loggerMock = new Mock<ILogger<GlobalExceptionMiddleware>>();
            var middleware = new GlobalExceptionMiddleware(
                next: async _ =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("secret sql connection error");
                },
                logger: loggerMock.Object);

            // Act
            await middleware.InvokeAsync(httpContext);

            // Assert
            httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            httpContext.Response.ContentType.Should().Contain("application/problem+json");

            using var doc = await ReadResponseJsonAsync(httpContext);
            var root = doc.RootElement;

            root.GetProperty("status").GetInt32().Should().Be(500);
            root.GetProperty("title").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("type").GetString().Should().NotBeNullOrWhiteSpace();
            root.GetProperty("instance").GetString().Should().Be("/api/v1/offers/1");
            root.GetProperty("hasError").GetBoolean().Should().BeTrue();

            var detail = root.GetProperty("detail").GetString();
            var error = root.GetProperty("error").GetString();

            detail.Should().NotBeNullOrWhiteSpace();
            detail.Should().NotContain("secret sql connection error");
            error.Should().NotBeNullOrWhiteSpace();
            error.Should().NotContain("secret sql connection error");
        }
    }
}
