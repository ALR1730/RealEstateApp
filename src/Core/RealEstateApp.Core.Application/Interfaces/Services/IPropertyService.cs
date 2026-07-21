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
    }
}
