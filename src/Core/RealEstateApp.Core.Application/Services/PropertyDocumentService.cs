using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Document;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Gestión de documentos legales de propiedades (Ítem 2.6).
    /// El agente sube la documentación; el agente propietario y el administrador la consultan.
    /// </summary>
    public class PropertyDocumentService : IPropertyDocumentService
    {
        private const string ContainerName = "property-documents";

        private readonly IPropertyDocumentRepository _documentRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<IdentityUser> _userManager;

        public PropertyDocumentService(
            IPropertyDocumentRepository documentRepository,
            IPropertyRepository propertyRepository,
            IFileStorageService fileStorageService,
            UserManager<IdentityUser> userManager)
        {
            _documentRepository = documentRepository;
            _propertyRepository = propertyRepository;
            _fileStorageService = fileStorageService;
            _userManager = userManager;
        }

        public async Task<PropertyDocumentViewModel> UploadAsync(
            int propertyId,
            string documentType,
            string uploadedBy,
            Stream fileStream,
            string fileName,
            string contentType)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                throw new NotFoundException("La propiedad no existe");

            if (property.AgentId != uploadedBy)
                throw new ValidationException("Solo puedes agregar documentos a tus propias propiedades.");

            if (!DocumentTypeConstants.IsValid(documentType))
                throw new ValidationException("Tipo de documento inválido.");

            if (fileStream == null || fileStream.Length == 0)
                throw new ValidationException("Debe seleccionar un archivo para subir.");

            if (fileStream.Length > 10 * 1024 * 1024)
                throw new ValidationException("El archivo no puede superar los 10 MB.");

            var fileUrl = await _fileStorageService.UploadFileAsync(fileStream, fileName, ContainerName);

            var document = new PropertyDocument
            {
                PropertyId = propertyId,
                DocumentType = documentType,
                FileUrl = fileUrl,
                OriginalFileName = fileName,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                SizeBytes = fileStream.Length,
                UploadedBy = uploadedBy
            };

            await _documentRepository.AddAsync(document);

            return await MapToViewModelAsync(document);
        }

        public async Task<List<PropertyDocumentViewModel>> GetByPropertyIdAsync(int propertyId, string currentUserId, bool isAdmin)
        {
            var property = await _propertyRepository.GetByIdAsync(propertyId);
            if (property == null)
                throw new NotFoundException("La propiedad no existe");

            if (!isAdmin && property.AgentId != currentUserId)
                throw new ValidationException("No tiene permisos para ver estos documentos.");

            var documents = await _documentRepository.GetByPropertyIdAsync(propertyId);
            return await MapToViewModelsAsync(documents);
        }

        public async Task<List<PropertyDocumentViewModel>> GetAllAsync()
        {
            var documents = await _documentRepository.GetAllAsync();
            return await MapToViewModelsAsync(documents);
        }

        public async Task DeleteAsync(int documentId, string currentUserId, bool isAdmin)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null)
                throw new NotFoundException("El documento no existe");

            if (!isAdmin && document.UploadedBy != currentUserId)
            {
                var property = await _propertyRepository.GetByIdAsync(document.PropertyId);
                if (property == null || property.AgentId != currentUserId)
                    throw new ValidationException("No tiene permisos para eliminar este documento.");
            }

            await _documentRepository.DeleteAsync(document);

            if (!string.IsNullOrEmpty(document.FileUrl))
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(document.FileUrl, ContainerName);
                }
                catch
                {
                    // El archivo físico no se encuentra: se ignora, el registro ya se eliminó.
                }
            }
        }

        private async Task<List<PropertyDocumentViewModel>> MapToViewModelsAsync(List<PropertyDocument> documents)
        {
            var result = new List<PropertyDocumentViewModel>();
            foreach (var d in documents)
            {
                result.Add(await MapToViewModelAsync(d));
            }
            return result;
        }

        private async Task<PropertyDocumentViewModel> MapToViewModelAsync(PropertyDocument d)
        {
            var vm = new PropertyDocumentViewModel
            {
                Id = d.Id,
                PropertyId = d.PropertyId,
                PropertyCode = d.Property?.Code,
                PropertyName = d.Property?.Name,
                DocumentType = d.DocumentType,
                FileUrl = d.FileUrl,
                OriginalFileName = d.OriginalFileName,
                ContentType = d.ContentType,
                SizeBytes = d.SizeBytes,
                UploadedBy = d.UploadedBy,
                UploadedAt = d.Created
            };

            var uploader = await _userManager.FindByIdAsync(d.UploadedBy);
            if (uploader != null)
            {
                vm.UploadedByName = !string.IsNullOrEmpty(uploader.UserName) ? uploader.UserName : uploader.Email;
            }

            return vm;
        }
    }
}