using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.DTOs.AiSearch;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Presentation.WebApi.Controllers.v1
{
    /// <summary>
    /// Controlador de Búsqueda Conversacional con Inteligencia Artificial (NLP).
    /// Permite a usuarios y clientes realizar búsquedas inteligentes mediante lenguaje natural libre.
    /// </summary>
    public class AiSearchController : BaseApiController
    {
        private readonly IAiSearchService _aiSearchService;

        public AiSearchController(IAiSearchService aiSearchService)
        {
            _aiSearchService = aiSearchService;
        }

        /// <summary>
        /// Procesa una consulta en lenguaje natural, extrae entidades semánticas y devuelve las propiedades encontradas.
        /// Endpoint público.
        /// </summary>
        [HttpPost("query")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AiSearchInterpretationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Query([FromBody] AiSearchQueryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _aiSearchService.SearchByNaturalLanguageAsync(request.Query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    hasError = true,
                    error = "Ocurrió un error al procesar la búsqueda semántica con IA: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Interpreta la consulta en lenguaje natural y extrae las entidades estructuradas sin consultar las propiedades.
        /// Útil para previsualizar los filtros que la IA aplicará.
        /// </summary>
        [HttpPost("interpret")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AiSearchInterpretationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Interpret([FromBody] AiSearchQueryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _aiSearchService.InterpretQueryAsync(request.Query);
            return Ok(result);
        }

        /// <summary>
        /// Devuelve una lista de sugerencias y ejemplos de prompts para la barra de búsqueda con IA.
        /// </summary>
        [HttpGet("suggestions")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AiSearchSuggestionsDto))]
        public IActionResult GetSuggestions()
        {
            var suggestions = _aiSearchService.GetPromptSuggestions();
            return Ok(new AiSearchSuggestionsDto { Suggestions = suggestions });
        }
    }
}
