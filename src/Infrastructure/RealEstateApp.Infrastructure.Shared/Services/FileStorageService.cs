using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    /// <summary>
    /// Servicio de almacenamiento de archivos con soporte local para desarrollo.
    /// En producción se puede reemplazar con una implementación de AWS S3 o Azure Blob Storage.
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
        private static readonly Regex SafeContainerRegex = new("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        private readonly string _basePath;

        /// <summary>
        /// Inicializa el servicio con la ruta base del proyecto web.
        /// Se espera que el directorio base sea "wwwroot" o equivalente.
        /// </summary>
        /// <param name="basePath">Ruta absoluta al directorio base (wwwroot).</param>
        public FileStorageService(string basePath)
        {
            _basePath = basePath;
        }

        /// <summary>
        /// Sube un archivo al almacenamiento local y retorna la URL relativa.
        /// </summary>
        /// <param name="fileStream">Stream del archivo a subir.</param>
        /// <param name="fileName">Nombre original del archivo.</param>
        /// <param name="containerName">Nombre del contenedor/carpeta (ej: "properties", "avatars").</param>
        /// <returns>URL relativa del archivo subido (ej: "/uploads/properties/guid_filename.jpg").</returns>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
        {
            ValidateContainerName(containerName);

            string extension = Path.GetExtension(fileName);
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";

            string baseFullPath = GetNormalizedBasePath();
            string uploadsDir = Path.GetFullPath(Path.Combine(baseFullPath, "uploads", containerName));
            EnsurePathInsideBase(uploadsDir, baseFullPath);

            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            string filePath = Path.GetFullPath(Path.Combine(uploadsDir, uniqueFileName));
            EnsurePathInsideBase(filePath, baseFullPath);

            using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            return $"/uploads/{containerName}/{uniqueFileName}";
        }

        /// <summary>
        /// Elimina un archivo del almacenamiento local previniendo path traversal.
        /// </summary>
        /// <param name="fileUrl">URL relativa del archivo a eliminar.</param>
        /// <param name="containerName">Nombre del contenedor.</param>
        public Task DeleteFileAsync(string fileUrl, string containerName)
        {
            ValidateContainerName(containerName);

            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                throw new ValidationException("La ruta del archivo es inválida.");
            }

            string relativePath = fileUrl
                .TrimStart('/', '\\')
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
            {
                throw new ValidationException("Ruta de archivo inválida.");
            }

            string baseFullPath = GetNormalizedBasePath();
            string fullPath = Path.GetFullPath(Path.Combine(baseFullPath, relativePath));
            EnsurePathInsideBase(fullPath, baseFullPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        private static void ValidateContainerName(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName) || !SafeContainerRegex.IsMatch(containerName))
            {
                throw new ValidationException("El nombre del contenedor de archivos es inválido.");
            }
        }

        private string GetNormalizedBasePath()
            => Path.GetFullPath(_basePath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

        private static void EnsurePathInsideBase(string candidateFullPath, string normalizedBasePathWithSeparator)
        {
            if (!candidateFullPath.StartsWith(normalizedBasePathWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Ruta de archivo inválida fuera del directorio permitido.");
            }
        }
    }
}
