using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.PropertyType;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para CRUD de tipos de propiedad.
    /// </summary>
    public interface IPropertyTypeService
    {
        Task<List<PropertyTypeViewModel>> GetAllViewModel();
        Task<SavePropertyTypeViewModel?> GetByIdSaveViewModel(int id);
        Task<SavePropertyTypeViewModel> Add(SavePropertyTypeViewModel vm);
        Task Update(SavePropertyTypeViewModel vm, int id);
        Task Delete(int id);
    }
}
