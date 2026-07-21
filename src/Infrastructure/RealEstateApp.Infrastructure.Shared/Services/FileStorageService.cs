using System;
using System.IO;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.Infrastructure.Shared.Services
{
    /// <summary>
    /// Servicio de almacenamiento de archivos con soporte local para desarrollo.
    /// En producción se puede reemplazar con una implementación de AWS S3 o Azure Blob Storage.
    /// </summary>
    public class FileStorageService : IFileStorageService
    {
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
            // Crear nombre único para evitar colisiones
            string extension = Path.GetExtension(fileName);
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Construir la ruta del directorio de destino
            string uploadsDir = Path.Combine(_basePath, "uploads", containerName);

            // Crear el directorio si no existe
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Ruta completa del archivo
            string filePath = Path.Combine(uploadsDir, uniqueFileName);

            // Escribir el archivo al disco
            using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            // Retornar URL relativa para uso en la aplicación
            string relativeUrl = $"/uploads/{containerName}/{uniqueFileName}";
            return relativeUrl;
        }

        /// <summary>
        /// Elimina un archivo del almacenamiento local.
        /// </summary>
        /// <param name="fileUrl">URL relativa del archivo a eliminar.</param>
        /// <param name="containerName">Nombre del contenedor (no se usa si la URL es completa).</param>
        public Task DeleteFileAsync(string fileUrl, string containerName)
        {
            // Convertir URL relativa a ruta del sistema de archivos
            // fileUrl viene como "/uploads/properties/guid_filename.jpg"
            string relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(_basePath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
