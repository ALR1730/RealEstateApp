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
    /// CRUD, gestión de imágenes, filtros combinados, búsqueda por código y gestión por agente.
    /// </summary>
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public PropertyService(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _fileStorageService = fileStorageService;
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

        public async Task<SavePropertyViewModel?> GetByIdSaveViewModel(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null) return null;

            var vm = _mapper.Map<SavePropertyViewModel>(property);
            var images = await _propertyImageRepository.GetByPropertyIdAsync(id);
            vm.ExistingImages = images.Select(i => i.ImageUrl).ToList();

            return vm;
        }

        public async Task<SavePropertyViewModel> Add(SavePropertyViewModel vm)
        {
            var property = _mapper.Map<Property>(vm);

            // Autogenerar código único de 6 dígitos y establecer estado
            property.Code = GenerateUniqueCode();
            property.Status = "Disponible";

            // Guardar entidad de propiedad
            property = await _propertyRepository.AddAsync(property);

            // Guardar imágenes si fueron subidas
            if (vm.Files != null && vm.Files.Count > 0)
            {
                foreach (var file in vm.Files.Take(15))
                {
                    if (file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, "properties");
                        await _propertyImageRepository.AddAsync(new PropertyImage
                        {
                            PropertyId = property.Id,
                            ImageUrl = imageUrl
                        });
                    }
                }
            }

            var result = _mapper.Map<SavePropertyViewModel>(property);
            return result;
        }

        public async Task Update(SavePropertyViewModel vm, int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new Exception($"No se encontró la propiedad con ID {id}");

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

            // Si se subieron nuevas imágenes
            if (vm.Files != null && vm.Files.Count > 0)
            {
                var existingImages = await _propertyImageRepository.GetByPropertyIdAsync(id);
                var currentCount = existingImages.Count;

                foreach (var file in vm.Files)
                {
                    if (currentCount >= 15) break;

                    if (file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, "properties");
                        await _propertyImageRepository.AddAsync(new PropertyImage
                        {
                            PropertyId = id,
                            ImageUrl = imageUrl
                        });
                        currentCount++;
                    }
                }
            }
        }

        public async Task Delete(int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new Exception($"No se encontró la propiedad con ID {id}");

            // Eliminar imágenes de disco e imágenes en BD
            var images = await _propertyImageRepository.GetByPropertyIdAsync(id);
            foreach (var img in images)
            {
                await _fileStorageService.DeleteFileAsync(img.ImageUrl, "properties");
                await _propertyImageRepository.DeleteAsync(img);
            }

            await _propertyRepository.DeleteAsync(property);
        }

        public async Task DeleteImage(int imageId)
        {
            var img = await _propertyImageRepository.GetByIdAsync(imageId);
            if (img != null)
            {
                await _fileStorageService.DeleteFileAsync(img.ImageUrl, "properties");
                await _propertyImageRepository.DeleteAsync(img);
            }
        }

        public async Task<List<PropertyViewModel>> GetAllWithFilters(PropertyFilterViewModel filters)
        {
            var allProperties = await _propertyRepository.GetAllAsync();

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

        private static string GenerateUniqueCode()
        {
            return Guid.NewGuid().ToString("N")[..6].ToUpper();
        }
    }
}
