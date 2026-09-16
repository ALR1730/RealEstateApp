using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Document;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IPropertyDocumentService
    {
        /// <summary>
        /// Sube un documento legal asociado a una propiedad.
        /// </summary>
        Task<PropertyDocumentViewModel> UploadAsync(
            int propertyId,
            string documentType,
            string uploadedBy,
            Stream fileStream,
            string fileName,
            string contentType);

        /// <summary>
        /// Documentos de una propiedad (Agente propietario o Administrador).
        /// </summary>
        Task<List<PropertyDocumentViewModel>> GetByPropertyIdAsync(int propertyId, string currentUserId, bool isAdmin);

        /// <summary>
        /// Todos los documentos del sistema (Administrador).
        /// </summary>
        Task<List<PropertyDocumentViewModel>> GetAllAsync();

        /// <summary>
        /// Elimina un documento (el agente que lo subió / propietario de la propiedad, o el Admin).
        /// </summary>
        Task DeleteAsync(int documentId, string currentUserId, bool isAdmin);
    }
}