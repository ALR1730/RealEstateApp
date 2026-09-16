using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.AiSearch;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de Búsqueda Conversacional con Inteligencia Artificial (NLP).
    /// Interpreta consultas libres en lenguaje natural y extrae filtros estructurados de propiedades.
    /// </summary>
    public interface IAiSearchService
    {
        /// <summary>
        /// Interpreta la consulta en lenguaje natural, extrae entidades y busca las propiedades coincidentes.
        /// </summary>
        Task<AiSearchInterpretationDto> SearchByNaturalLanguageAsync(string query);

        /// <summary>
        /// Interpreta la consulta en lenguaje natural y extrae las entidades estructuradas sin consultar la base de datos.
        /// </summary>
        Task<AiSearchInterpretationDto> InterpretQueryAsync(string query);

        /// <summary>
        /// Obtiene una lista de sugerencias de búsqueda guiadas para la interfaz gráfica.
        /// </summary>
        List<string> GetPromptSuggestions();
    }
}
