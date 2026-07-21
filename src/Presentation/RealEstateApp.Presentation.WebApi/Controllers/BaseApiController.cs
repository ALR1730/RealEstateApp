using Microsoft.AspNetCore.Mvc;

namespace RealEstateApp.Presentation.WebApi.Controllers
{
    /// <summary>
    /// Controlador base para los endpoints de la Web API.
    /// Define la ruta estándar api/v1/[controller] y el atributo [ApiController].
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
    }
}
