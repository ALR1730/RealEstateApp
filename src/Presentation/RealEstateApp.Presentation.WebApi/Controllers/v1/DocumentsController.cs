using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Document;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    [Authorize(Roles = "Agent,Admin")]
    public class DocumentsController : BaseApiController
    {
        private readonly IPropertyDocumentService _documentService;

        public DocumentsController(IPropertyDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Sube un documento legal asociado a una propiedad (Agente propietario).
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { hasError = true, error = "Debe seleccionar un archivo." });
            }

            try
            {
                using var stream = request.File.OpenReadStream();
                var doc = await _documentService.UploadAsync(
                    request.PropertyId,
                    request.DocumentType,
                    currentUserId,
                    stream,
                    request.File.FileName,
                    request.File.ContentType);

                return Ok(new { success = true, message = "Documento subido exitosamente.", document = doc });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { hasError = true, error = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Documentos de una propiedad (Agente propietario o Administrador).
        /// </summary>
        [HttpGet("property/{propertyId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByProperty(int propertyId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");

            try
            {
                var documents = await _documentService.GetByPropertyIdAsync(propertyId, currentUserId, isAdmin);
                return Ok(documents);
            }
            catch (ValidationException ex)
            {
                return Forbid(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
        }

        /// <summary>
        /// Todos los documentos del sistema (Administrador).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var documents = await _documentService.GetAllAsync();
            return Ok(documents);
        }

        /// <summary>
        /// Elimina un documento (el agente que lo subió / propietario, o el Administrador).
        /// </summary>
        [HttpDelete("{documentId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int documentId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");

            try
            {
                await _documentService.DeleteAsync(documentId, currentUserId, isAdmin);
                return Ok(new { success = true, message = "Documento eliminado exitosamente." });
            }
            catch (ValidationException ex)
            {
                return Forbid(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { hasError = true, error = ex.Message });
            }
        }

        public class UploadDocumentRequest
        {
            public int PropertyId { get; set; }
            public string DocumentType { get; set; } = string.Empty;
            public IFormFile? File { get; set; }
        }
    }
}