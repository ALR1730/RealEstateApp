using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.SaleType;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class SaleTypeService : ISaleTypeService
    {
        private readonly IGenericRepository<SaleType> _repository;
        private readonly IMapper _mapper;

        public SaleTypeService(IGenericRepository<SaleType> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<SaleTypeViewModel>> GetAllViewModel()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<List<SaleTypeViewModel>>(list);
        }

        public async Task<SaveSaleTypeViewModel?> GetByIdSaveViewModel(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<SaveSaleTypeViewModel>(entity);
        }

        public async Task<SaveSaleTypeViewModel> Add(SaveSaleTypeViewModel vm)
        {
            var entity = _mapper.Map<SaleType>(vm);
            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SaveSaleTypeViewModel>(entity);
        }

        public async Task Update(SaveSaleTypeViewModel vm, int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return;

            entity.Name = vm.Name;
            entity.Description = vm.Description;
            await _repository.UpdateAsync(entity);
        }

        public async Task Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return;
            await _repository.DeleteAsync(entity);
        }
    }
}
