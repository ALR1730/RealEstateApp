using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Improvement;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para CRUD de mejoras.
    /// </summary>
    public interface IImprovementService
    {
        Task<List<ImprovementViewModel>> GetAllViewModel();
        Task<SaveImprovementViewModel?> GetByIdSaveViewModel(int id);
        Task<SaveImprovementViewModel> Add(SaveImprovementViewModel vm);
        Task Update(SaveImprovementViewModel vm, int id);
        Task Delete(int id);
    }
}
