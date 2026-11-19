using System;
using System.IO;

namespace RealEstateApp.Core.Application.DTOs.Common
{
    /// <summary>
    /// Abstracción de archivo subido independiente de HTTP/ASP.NET Core para mantener limpia la capa Application (BETA).
    /// </summary>
    public sealed class FileUploadDto : IDisposable
    {
        public Stream Content { get; }
        public string FileName { get; }
        public string ContentType { get; }
        public long Length { get; }

        public FileUploadDto(Stream content, string fileName, string contentType, long length)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            FileName = fileName ?? string.Empty;
            ContentType = contentType ?? string.Empty;
            Length = length;
        }

        public void Dispose()
        {
            Content.Dispose();
        }
    }
}
