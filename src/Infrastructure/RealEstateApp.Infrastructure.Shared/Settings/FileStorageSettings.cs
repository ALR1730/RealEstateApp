namespace RealEstateApp.Infrastructure.Shared.Settings
{
    /// <summary>
    /// Configuración para el servicio de almacenamiento de archivos.
    /// Se lee desde appsettings.json sección "FileStorageSettings".
    /// </summary>
    public class FileStorageSettings
    {
        /// <summary>
        /// Directorio base donde se almacenan los archivos subidos (ej: "wwwroot").
        /// </summary>
        public string BasePath { get; set; } = "wwwroot";

        /// <summary>
        /// URL base para construir las rutas públicas de los archivos.
        /// </summary>
        public string BaseUrl { get; set; } = "";

        /// <summary>
        /// Extensiones de archivo permitidas para imágenes.
        /// </summary>
        public string[] AllowedImageExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        /// <summary>
        /// Tamaño máximo de archivo en bytes (por defecto 5 MB).
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    }
}
