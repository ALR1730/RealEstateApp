using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Presentation.WebApi.Middleware;

namespace RealEstateApp.Presentation.WebApi.Filters
{
    /// <summary>
    /// Filtro global de excepciones de la API.
    /// Traduce excepciones conocidas de dominio y concurrencia a ProblemDetails RFC 7807.
    /// Las excepciones desconocidas se delegan a GlobalExceptionMiddleware.
    /// </summary>
    public class ApiGlobalExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;
            var instance = context.HttpContext?.Request?.Path.Value;

            if (exception is ValidationException)
            {
                var problem = GlobalExceptionMiddleware.CreateProblemDetails(exception, instance);
                context.Result = new BadRequestObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                context.ExceptionHandled = true;
            }
            else if (exception is NotFoundException)
            {
                var problem = GlobalExceptionMiddleware.CreateProblemDetails(exception, instance);
                context.Result = new NotFoundObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
                context.ExceptionHandled = true;
            }
            else if (exception is DomainException)
            {
                var problem = GlobalExceptionMiddleware.CreateProblemDetails(exception, instance);
                context.Result = new UnprocessableEntityObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };
                context.ExceptionHandled = true;
            }
            else if (exception is DbUpdateConcurrencyException)
            {
                var problem = GlobalExceptionMiddleware.CreateProblemDetails(exception, instance);
                context.Result = new ConflictObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
                context.ExceptionHandled = true;
            }
            else if (exception is UnauthorizedAccessException)
            {
                var problem = GlobalExceptionMiddleware.CreateProblemDetails(exception, instance);
                context.Result = new ObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                context.ExceptionHandled = true;
            }

            return Task.CompletedTask;
        }
    }
}
