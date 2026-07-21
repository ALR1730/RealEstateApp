using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.SaleType;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para CRUD de tipos de venta.
    /// </summary>
    public interface ISaleTypeService
    {
        Task<List<SaleTypeViewModel>> GetAllViewModel();
        Task<SaveSaleTypeViewModel?> GetByIdSaveViewModel(int id);
        Task<SaveSaleTypeViewModel> Add(SaveSaleTypeViewModel vm);
        Task Update(SaveSaleTypeViewModel vm, int id);
        Task Delete(int id);
    }
}
