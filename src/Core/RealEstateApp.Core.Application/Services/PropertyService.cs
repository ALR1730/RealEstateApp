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
        private readonly IPropertyTypeRepository _propertyTypeRepository;
        private readonly IPropertyImprovementRepository _propertyImprovementRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public PropertyService(
            IPropertyRepository propertyRepository,
            IPropertyImageRepository propertyImageRepository,
            IPropertyTypeRepository propertyTypeRepository,
            IPropertyImprovementRepository propertyImprovementRepository,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _propertyImageRepository = propertyImageRepository;
            _propertyTypeRepository = propertyTypeRepository;
            _propertyImprovementRepository = propertyImprovementRepository;
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

            var improvements = await _propertyImprovementRepository.GetByPropertyIdAsync(id);
            vm.ImprovementIds = improvements.Select(pi => pi.ImprovementId).ToList();

            return vm;
        }

        public async Task<SavePropertyViewModel> Add(SavePropertyViewModel vm)
        {
            var property = _mapper.Map<Property>(vm);
            property.Name = vm.Name;

            // Obtener el tipo de propiedad para generar dinámicamente su prefijo/identificador
            var propertyType = await _propertyTypeRepository.GetByIdAsync(vm.PropertyTypeId);
            string prefix = GetPropertyTypePrefix(propertyType?.Name);

            // Autogenerar código único con prefijo de tipo de propiedad
            property.Code = await GenerateUniqueCodeAsync(prefix);
            property.Status = PropertyStatus.Available;
            property.AgentId = vm.AgentId;

            // Guardar entidad de propiedad
            property = await _propertyRepository.AddAsync(property);

            // Guardar mejoras seleccionadas
            if (vm.ImprovementIds != null && vm.ImprovementIds.Count > 0)
            {
                await _propertyImprovementRepository.UpdatePropertyImprovementsAsync(property.Id, vm.ImprovementIds);
            }

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

            property.Name = vm.Name;
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
            property.MatterportModelId = vm.MatterportModelId;
            property.MontoSeparacion = vm.MontoSeparacion;
            property.PorcentajeInicialRequerido = vm.PorcentajeInicialRequerido;
            property.IsFinanciable = vm.IsFinanciable;
            property.ProvinceId = vm.ProvinceId;
            property.MunicipalityId = vm.MunicipalityId;
            property.Sector = vm.Sector;
            property.FullAddress = vm.FullAddress;
            property.IsFeatured = vm.IsFeatured;
            property.FeaturedUntil = vm.FeaturedUntil;

            await _propertyRepository.UpdateAsync(property);

            // Actualizar mejoras asociadas
            if (vm.ImprovementIds != null)
            {
                await _propertyImprovementRepository.UpdatePropertyImprovementsAsync(id, vm.ImprovementIds);
            }

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

        public async Task ToggleFeaturedAsync(int propertyId, int durationDays = 30)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                throw new NotFoundException($"No se encontró la propiedad con ID {propertyId}");

            if (property.IsFeatured && (!property.FeaturedUntil.HasValue || property.FeaturedUntil > DateTime.UtcNow))
            {
                // Desactivar destacado
                property.IsFeatured = false;
                property.FeaturedUntil = null;
            }
            else
            {
                // Activar destacado
                property.IsFeatured = true;
                property.FeaturedUntil = DateTime.UtcNow.AddDays(durationDays);
            }

            await _propertyRepository.UpdateAsync(property);
        }

        public async Task<List<string>> GetDistinctSectorsAsync(int? provinceId = null, int? municipalityId = null)
        {
            return await _propertyRepository.GetDistinctSectorsAsync(provinceId, municipalityId);
        }

        private async Task<string> GenerateUniqueCodeAsync(string prefix)
        {
            string code;
            do
            {
                code = $"{prefix}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }
            while (await _propertyRepository.GetByCodeAsync(code) != null);

            return code;
        }

        private string GetPropertyTypePrefix(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "PROP";

            var trimmed = typeName.Trim();
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length > 1)
            {
                var initials = new string(words.Select(w => char.ToUpper(w[0])).ToArray());
                if (initials.Length >= 3)
                    return initials[..3];

                var firstWordChar = char.ToUpper(words[0][0]);
                var secondWordClean = new string(words[1].Where(char.IsLetterOrDigit).ToArray()).ToUpper();
                if (secondWordClean.Length >= 2)
                    return $"{firstWordChar}{secondWordClean[..2]}";

                return (firstWordChar + secondWordClean).PadRight(3, 'X');
            }

            var cleanWord = new string(trimmed.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            if (cleanWord.Length <= 3)
                return cleanWord.PadRight(3, 'X');

            // Coincidencias conocidas comunes para alta legibilidad
            if (cleanWord.StartsWith("APARTAM")) return "APT";
            if (cleanWord.StartsWith("VILL")) return "VIL";
            if (cleanWord.StartsWith("CASA")) return "CAS";
            if (cleanWord.StartsWith("PENTH")) return "PNT";
            if (cleanWord.StartsWith("TERRE")) return "TER";
            if (cleanWord.StartsWith("LOCAL")) return "LOC";
            if (cleanWord.StartsWith("EDIFI")) return "EDI";

            // Algoritmo dinámico para cualquier tipo de propiedad nuevo
            var firstChar = cleanWord[0];
            var consonants = cleanWord.Substring(1).Where(c => !"AEIOUáéíóúÁÉÍÓÚ".Contains(c)).ToArray();
            if (consonants.Length >= 2)
            {
                return $"{firstChar}{consonants[0]}{consonants[1]}";
            }

            return cleanWord[..3];
        }
    }
}
