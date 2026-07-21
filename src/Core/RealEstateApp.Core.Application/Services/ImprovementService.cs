using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Improvement;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class ImprovementService : IImprovementService
    {
        private readonly IGenericRepository<Improvement> _repository;
        private readonly IMapper _mapper;

        public ImprovementService(IGenericRepository<Improvement> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ImprovementViewModel>> GetAllViewModel()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<List<ImprovementViewModel>>(list);
        }

        public async Task<SaveImprovementViewModel?> GetByIdSaveViewModel(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<SaveImprovementViewModel>(entity);
        }

        public async Task<SaveImprovementViewModel> Add(SaveImprovementViewModel vm)
        {
            var entity = _mapper.Map<Improvement>(vm);
            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SaveImprovementViewModel>(entity);
        }

        public async Task Update(SaveImprovementViewModel vm, int id)
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
