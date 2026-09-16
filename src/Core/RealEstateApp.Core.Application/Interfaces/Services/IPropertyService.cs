using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Property;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para gestión de propiedades.
    /// CRUD, filtros combinados, gestión de imágenes y cambio de estado.
    /// </summary>
    public interface IPropertyService
    {
        Task<List<PropertyViewModel>> GetAllViewModel();
        Task<PropertyViewModel?> GetByIdViewModel(int id);
        Task<SavePropertyViewModel?> GetByIdSaveViewModel(int id);
        Task<SavePropertyViewModel> Add(SavePropertyViewModel vm);
        Task Update(SavePropertyViewModel vm, int id);
        Task Delete(int id);
        Task DeleteImage(int imageId);

        /// <summary>
        /// Filtra propiedades por criterios combinados.
        /// </summary>
        Task<List<PropertyViewModel>> GetAllWithFilters(PropertyFilterViewModel filters);

        /// <summary>
        /// Obtiene propiedades de un agente específico.
        /// </summary>
        Task<List<PropertyViewModel>> GetByAgentId(string agentId);

        /// <summary>
        /// Busca una propiedad por su código único de 6 dígitos.
        /// </summary>
        Task<PropertyViewModel?> GetByCode(string code);

        /// <summary>
        /// Reasigna una propiedad a un nuevo agente.
        /// </summary>
        Task ReassignAgent(int propertyId, string newAgentId);

        /// <summary>
        /// Alterna el estado de propiedad destacada (Featured) por una cantidad de días.
        /// </summary>
        Task ToggleFeaturedAsync(int propertyId, int durationDays = 30);

        /// <summary>
        /// Obtiene la lista de sectores únicos registrados en las propiedades de la base de datos.
        /// </summary>
        Task<List<string>> GetDistinctSectorsAsync(int? provinceId = null, int? municipalityId = null);

        /// <summary>
        /// Obtiene el historial de precios y análisis de tendencias de una propiedad.
        /// </summary>
        Task<List<PriceHistoryViewModel>> GetPriceHistoryAsync(int propertyId);
    }
}
