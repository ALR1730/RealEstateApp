using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Presentation.WebApi.Filters
{
    /// <summary>
    /// Filtro global de excepciones de la API.
    /// Traduce las excepciones de dominio a respuestas HTTP con el código adecuado
    /// (ValidationException → 400, NotFoundException → 404) en lugar de devolver 500.
    /// </summary>
    public class ApiGlobalExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;

            if (exception is ValidationException validationException)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    hasError = true,
                    error = validationException.Message
                });
                context.ExceptionHandled = true;
            }
            else if (exception is NotFoundException notFoundException)
            {
                context.Result = new NotFoundObjectResult(new
                {
                    hasError = true,
                    error = notFoundException.Message
                });
                context.ExceptionHandled = true;
            }

            return Task.CompletedTask;
        }
    }
}