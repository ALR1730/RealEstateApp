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
        private const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Whitelist de MIME types permitidos para imágenes de propiedades.
        /// OWASP: la validación de MIME siempre se realiza en el servidor, nunca se confía en el cliente.
        /// </summary>
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        /// <summary>
        /// Firmas mágicas (magic bytes) de los formatos permitidos.
        /// Previene bypass de Content-Type con archivos renombrados (ej: malware.jpg).
        /// </summary>
        private static readonly List<byte[]> AllowedSignatures = new()
        {
            new byte[] { 0xFF, 0xD8, 0xFF },              // JPEG
            new byte[] { 0x89, 0x50, 0x4E, 0x47 },        // PNG
            new byte[] { 0x52, 0x49, 0x46, 0x46 }         // WebP (RIFF header)
        };

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
                if (file.Length <= 0) continue;

                // Guardia 1: Tamaño máximo (10 MB)
                if (file.Length > MaxImageSizeBytes)
                {
                    _logger.LogWarning("Archivo rechazado: tamaño {Size} bytes supera el límite de {Max} bytes. Propiedad {PropertyId}.",
                        file.Length, MaxImageSizeBytes, propertyId);
                    continue;
                }

                // Guardia 2: MIME type declarado en el header HTTP
                if (!AllowedMimeTypes.Contains(file.ContentType))
                {
                    _logger.LogWarning("Archivo rechazado: MIME type '{ContentType}' no permitido. Propiedad {PropertyId}.",
                        file.ContentType, propertyId);
                    continue;
                }

                // Guardia 3: Firma mágica del stream (previene renombrado de archivos maliciosos)
                if (!await HasAllowedMagicBytesAsync(file))
                {
                    _logger.LogWarning("Archivo rechazado: firma binaria no coincide con ningún formato de imagen permitido. Propiedad {PropertyId}.",
                        propertyId);
                    continue;
                }

                using var stream = file.OpenReadStream();
                var imageUrl = await _fileStorageService.UploadFileAsync(stream, file.FileName, ContainerName);
                imageEntities.Add(new PropertyImage
                {
                    PropertyId = propertyId,
                    ImageUrl = imageUrl
                });
            }

            if (imageEntities.Count > 0)
            {
                await _propertyImageRepository.AddRangeAsync(imageEntities);
            }

            return imageEntities;
        }

        /// <summary>
        /// Verifica que los primeros bytes del archivo coincidan con una firma conocida.
        /// Esto previene ataques de bypass donde se renombra un archivo ejecutable con extensión .jpg.
        /// </summary>
        private static async Task<bool> HasAllowedMagicBytesAsync(IFormFile file)
        {
            var header = new byte[4];
            using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(header, 0, header.Length);
            if (bytesRead < 3) return false;

            return AllowedSignatures.Any(sig =>
                header.Take(sig.Length).SequenceEqual(sig));
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
