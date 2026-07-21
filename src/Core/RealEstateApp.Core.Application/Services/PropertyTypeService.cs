using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.PropertyType;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class PropertyTypeService : IPropertyTypeService
    {
        private readonly IGenericRepository<PropertyType> _repository;
        private readonly IMapper _mapper;

        public PropertyTypeService(IGenericRepository<PropertyType> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<PropertyTypeViewModel>> GetAllViewModel()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<List<PropertyTypeViewModel>>(list);
        }

        public async Task<SavePropertyTypeViewModel?> GetByIdSaveViewModel(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<SavePropertyTypeViewModel>(entity);
        }

        public async Task<SavePropertyTypeViewModel> Add(SavePropertyTypeViewModel vm)
        {
            var entity = _mapper.Map<PropertyType>(vm);
            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SavePropertyTypeViewModel>(entity);
        }

        public async Task Update(SavePropertyTypeViewModel vm, int id)
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
