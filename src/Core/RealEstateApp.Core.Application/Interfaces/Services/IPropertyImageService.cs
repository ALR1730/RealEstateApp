using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Contrato de servicio especializado en el ciclo de vida y almacenamiento de imágenes de propiedades.
    /// </summary>
    public interface IPropertyImageService
    {
        Task<List<PropertyImage>> SaveImagesAsync(int propertyId, IReadOnlyList<IFormFile>? files, int maxImages = 15);
        Task DeletePhysicalImagesAsync(IEnumerable<PropertyImage>? images);
        Task DeleteImageByIdAsync(int imageId);
    }
}
