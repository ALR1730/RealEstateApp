using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

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

            // Autogenerar código único de 6 dígitos con verificación de unicidad en BD
            property.Code = await GenerateUniqueCodeAsync();
            property.Status = PropertyStatus.Available;
            property.AgentId = vm.AgentId;

            // Guardar entidad de propiedad
            property = await _propertyRepository.AddAsync(property);

            // Guardar imágenes si fueron subidas en lote
            if (vm.Files != null && vm.Files.Count > 0)
            {
                var imageEntities = new List<PropertyImage>();
                foreach (var file in vm.Files.Take(15))
                {
                    if (file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, "properties");
                        imageEntities.Add(new PropertyImage
                        {
                            PropertyId = property.Id,
                            ImageUrl = imageUrl
                        });
                    }
                }
                if (imageEntities.Any())
                {
                    await _propertyImageRepository.AddRangeAsync(imageEntities);
                }
            }

            var result = _mapper.Map<SavePropertyViewModel>(property);
            return result;
        }

        public async Task Update(SavePropertyViewModel vm, int id)
        {
            var property = await _propertyRepository.GetByIdAsync(id);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {id}");

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
            property.IsFinanciable = vm.IsFinanciable;

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
                throw new NotFoundException($"No se encontró la propiedad con ID {id}");

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
            var properties = await _propertyRepository.GetWithFiltersAsync(filters);
            return _mapper.Map<List<PropertyViewModel>>(properties);
        }

        public async Task<List<PropertyViewModel>> GetByAgentId(string agentId)
        {
            var properties = await _propertyRepository.GetByAgentIdAsync(agentId);
            return _mapper.Map<List<PropertyViewModel>>(properties);
        }

        public async Task<PropertyViewModel?> GetByCode(string code)
        {
            var property = await _propertyRepository.GetByCodeAsync(code);
            if (property == null) return null;
            return _mapper.Map<PropertyViewModel>(property);
        }

        public async Task ReassignAgent(int propertyId, string newAgentId)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {propertyId}");

            property.AgentId = newAgentId;
            await _propertyRepository.UpdateAsync(property);
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            string code;
            do
            {
                code = Guid.NewGuid().ToString("N")[..6].ToUpper();
            }
            while (await _propertyRepository.GetByCodeAsync(code) != null);

            return code;
        }
    }
}
