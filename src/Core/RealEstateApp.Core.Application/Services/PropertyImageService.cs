using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio enfocado exclusivamente en la gestión de imágenes físicas y persistencia asociada.
    /// Cumple con Single Responsibility Principle (SRP).
    /// </summary>
    public class PropertyImageService : IPropertyImageService
    {
        private const string ContainerName = "properties";
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<PropertyImageService> _logger;

        public PropertyImageService(
            IPropertyImageRepository propertyImageRepository,
            IFileStorageService fileStorageService,
            ILogger<PropertyImageService>? logger = null)
        {
            _propertyImageRepository = propertyImageRepository;
            _fileStorageService = fileStorageService;
            _logger = logger ?? NullLogger<PropertyImageService>.Instance;
        }

        public async Task<List<PropertyImage>> SaveImagesAsync(int propertyId, IReadOnlyList<IFormFile>? files, int maxImages = 15)
        {
            if (files == null || files.Count == 0) return new List<PropertyImage>();

            var existingImages = await _propertyImageRepository.GetByPropertyIdAsync(propertyId);
            int remainingSlots = Math.Max(0, maxImages - existingImages.Count);

            var imageEntities = new List<PropertyImage>();
            foreach (var file in files.Take(remainingSlots))
            {
                if (file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, ContainerName);
                    imageEntities.Add(new PropertyImage
                    {
                        PropertyId = propertyId,
                        ImageUrl = imageUrl
                    });
                }
            }

            if (imageEntities.Count > 0)
            {
                await _propertyImageRepository.AddRangeAsync(imageEntities);
            }

            return imageEntities;
        }

        public async Task DeletePhysicalImagesAsync(IEnumerable<PropertyImage>? images)
        {
            if (images == null) return;

            foreach (var img in images)
            {
                if (!string.IsNullOrEmpty(img?.ImageUrl))
                {
                    try
                    {
                        await _fileStorageService.DeleteFileAsync(img.ImageUrl, ContainerName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Fallo al eliminar archivo físico de imagen {ImageUrl}", img.ImageUrl);
                    }
                }
            }
        }

        public async Task DeleteImageByIdAsync(int imageId)
        {
            var image = await _propertyImageRepository.GetByIdAsync(imageId);
            if (image != null)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(image.ImageUrl, ContainerName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Fallo al eliminar archivo físico de imagen {ImageUrl}", image.ImageUrl);
                }
                await _propertyImageRepository.DeleteAsync(image);
            }
        }
    }
}
