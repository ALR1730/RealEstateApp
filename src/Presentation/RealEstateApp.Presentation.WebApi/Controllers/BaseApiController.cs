using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Domain.Common;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    /// <summary>
    /// Controlador base para los endpoints de la Web API.
    /// Define la ruta estándar api/v1/[controller], el atributo [ApiController] y helpers RFC 7807 / identidad.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected string? CurrentUserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

        protected IActionResult ToActionResult(Result result)
        {
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return CreateProblemResult(result.Error);
        }

        protected IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return CreateProblemResult(result.Error);
        }

        private ObjectResult CreateProblemResult(Error error)
        {
            var (status, type, title) = error.Type switch
            {
                ErrorType.Validation => (
                    StatusCodes.Status400BadRequest,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    "Error de validación"),
                ErrorType.NotFound => (
                    StatusCodes.Status404NotFound,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    "Recurso no encontrado"),
                ErrorType.Conflict => (
                    StatusCodes.Status409Conflict,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    "Conflicto de estado"),
                ErrorType.Forbidden => (
                    StatusCodes.Status403Forbidden,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    "Acceso denegado"),
                ErrorType.Unauthorized => (
                    StatusCodes.Status401Unauthorized,
                    "https://tools.ietf.org/html/rfc7235#section-3.1",
                    "No autorizado"),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    "Error interno del servidor")
            };

            var problem = new ProblemDetails
            {
                Type = type,
                Title = title,
                Status = status,
                Detail = error.Message,
                Instance = HttpContext?.Request?.Path.Value
            };

            problem.Extensions["code"] = error.Code;
            problem.Extensions["hasError"] = true;
            problem.Extensions["error"] = error.Message;

            return new ObjectResult(problem)
            {
                StatusCode = status
            };
        }
    }
}
