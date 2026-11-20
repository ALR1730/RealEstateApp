using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Presentation.WebApi.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await WriteProblemDetailsResponseAsync(context, ex);
            }
        }

        public static ProblemDetails CreateProblemDetails(Exception exception, string? instance)
        {
            var (status, type, title, detail) = exception switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    "Error de validación",
                    validationEx.Message),

                NotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    "Recurso no encontrado",
                    notFoundEx.Message),

                DomainException domainEx => (
                    StatusCodes.Status422UnprocessableEntity,
                    "https://tools.ietf.org/html/rfc4918#section-11.2",
                    "Regla de negocio incumplida",
                    domainEx.Message),

                UnauthorizedAccessException => (
                    StatusCodes.Status403Forbidden,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    "Acceso denegado",
                    "No tiene permisos suficientes para realizar esta acción."),

                DbUpdateConcurrencyException => (
                    StatusCodes.Status409Conflict,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    "Conflicto de concurrencia",
                    "El recurso fue modificado por otro usuario. Recargue e intente nuevamente."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    "Error interno del servidor",
                    "Ha ocurrido un error interno inesperado. Intente nuevamente más tarde.")
            };

            var problemDetails = new ProblemDetails
            {
                Type = type,
                Title = title,
                Status = status,
                Detail = detail,
                Instance = instance
            };

            problemDetails.Extensions["hasError"] = true;
            problemDetails.Extensions["error"] = detail;

            return problemDetails;
        }

        private async Task WriteProblemDetailsResponseAsync(HttpContext context, Exception exception)
        {
            var problemDetails = CreateProblemDetails(exception, context.Request.Path);
            var statusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Error interno no controlado en {Path}", context.Request.Path);
            }
            else
            {
                _logger.LogWarning(exception, "Excepción controlada ({StatusCode}) en {Path}: {Message}",
                    statusCode, context.Request.Path, exception.Message);
            }

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json; charset=utf-8";

            var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
