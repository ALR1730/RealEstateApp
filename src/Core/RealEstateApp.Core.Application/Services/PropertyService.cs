using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para propiedades.
    /// CRUD, filtros combinados, búsqueda por código y gestión por agente.
    /// </summary>
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IMapper _mapper;

        public PropertyService(IPropertyRepository propertyRepository, IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _mapper = mapper;
        }

        public async Task<List<PropertyViewModel>> GetAllViewModel()
        {
            var properties = await _propertyRepository.GetAllAsync();
            return _mapper.Map<List<PropertyViewModel>>(properties);
        }

        public async Task<PropertyViewModel?> GetByIdViewModel(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return null;
            return _mapper.Map<PropertyViewModel>(property);
        }

        public async Task<SavePropertyViewModel> Add(SavePropertyViewModel vm)
        {
            var property = _mapper.Map<Property>(vm);

            // Autogenerar código único de 6 dígitos
            property.Code = GenerateUniqueCode();
            property.Status = "Disponible";

            property = await _propertyRepository.AddAsync(property);

            // Retornar el VM con el ID generado
            var result = _mapper.Map<SavePropertyViewModel>(property);
            return result;
        }

        public async Task Update(SavePropertyViewModel vm, int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new Exception($"No se encontró la propiedad con ID {id}");

            // Mapear solo los campos editables, preservar Code, AgentId, Status
            property.Price = vm.Price;
            property.Rooms = vm.Rooms;
            property.Bathrooms = vm.Bathrooms;
            property.SizeInMeters = vm.SizeInMeters;
            property.Description = vm.Description;
            property.PropertyTypeId = vm.PropertyTypeId;
            property.SaleTypeId = vm.SaleTypeId;
            property.Latitude = vm.Latitude;
            property.Longitude = vm.Longitude;
            property.VideoUrl = vm.VideoUrl;
            property.Tour360Url = vm.Tour360Url;
            property.MontoSeparacion = vm.MontoSeparacion;
            property.PorcentajeInicialRequerido = vm.PorcentajeInicialRequerido;

            await _propertyRepository.UpdateAsync(property);
        }

        public async Task Delete(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new Exception($"No se encontró la propiedad con ID {id}");

            await _propertyRepository.DeleteAsync(property);
        }

        public async Task<List<PropertyViewModel>> GetAllWithFilters(PropertyFilterViewModel filters)
        {
            var allProperties = await _propertyRepository.GetAllAsync();

            // Aplicar filtros combinados
            var query = allProperties.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filters.Code))
                query = query.Where(p => p.Code.Equals(filters.Code, StringComparison.OrdinalIgnoreCase));

            if (filters.PropertyTypeId.HasValue)
                query = query.Where(p => p.PropertyTypeId == filters.PropertyTypeId.Value);

            if (filters.SaleTypeId.HasValue)
                query = query.Where(p => p.SaleTypeId == filters.SaleTypeId.Value);

            if (filters.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filters.MinPrice.Value);

            if (filters.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filters.MaxPrice.Value);

            if (filters.MinRooms.HasValue)
                query = query.Where(p => p.Rooms >= filters.MinRooms.Value);

            if (filters.MaxRooms.HasValue)
                query = query.Where(p => p.Rooms <= filters.MaxRooms.Value);

            if (filters.MinBathrooms.HasValue)
                query = query.Where(p => p.Bathrooms >= filters.MinBathrooms.Value);

            if (filters.MaxBathrooms.HasValue)
                query = query.Where(p => p.Bathrooms <= filters.MaxBathrooms.Value);

            if (!string.IsNullOrWhiteSpace(filters.AgentId))
                query = query.Where(p => p.AgentId == filters.AgentId);

            return _mapper.Map<List<PropertyViewModel>>(query.ToList());
        }

        public async Task<List<PropertyViewModel>> GetByAgentId(string agentId)
        {
            var allProperties = await _propertyRepository.GetAllAsync();
            var agentProperties = allProperties.Where(p => p.AgentId == agentId).ToList();
            return _mapper.Map<List<PropertyViewModel>>(agentProperties);
        }

        public async Task<PropertyViewModel?> GetByCode(string code)
        {
            var allProperties = await _propertyRepository.GetAllAsync();
            var property = allProperties.FirstOrDefault(p =>
                p.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

            if (property == null) return null;
            return _mapper.Map<PropertyViewModel>(property);
        }

        /// <summary>
        /// Genera un código alfanumérico único de 6 dígitos.
        /// </summary>
        private static string GenerateUniqueCode()
        {
            return Guid.NewGuid().ToString("N")[..6].ToUpper();
        }
    }
}
