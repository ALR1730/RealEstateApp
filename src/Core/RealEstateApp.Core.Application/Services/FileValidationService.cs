using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Validador centralizado de archivos subidos (Triple Barrera: tamaño/extensión, MIME y firma binaria).
    /// </summary>
    public class FileValidationService : IFileValidationService
    {
        public const long MaxPropertyImageBytes = 10L * 1024 * 1024;
        public const long MaxVerificationImageBytes = 5L * 1024 * 1024;
        public const long MaxLegalDocumentBytes = 10L * 1024 * 1024;

        private const int HeaderInspectLength = 12;

        private sealed record FormatRule(
            HashSet<string> AllowedMimeTypes,
            Func<byte[], int, bool> MatchesMagicBytes);

        private sealed record PolicyRule(
            long MaxSizeBytes,
            HashSet<string> AllowedExtensions);

        private static readonly FormatRule JpegRule = new(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/jpg" },
            IsJpeg);

        private static readonly FormatRule PngRule = new(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/png" },
            IsPng);

        private static readonly FormatRule WebpRule = new(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/webp" },
            IsWebp);

        private static readonly FormatRule PdfRule = new(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "application/pdf" },
            IsPdf);

        private static readonly Dictionary<string, FormatRule> FormatRulesByExtension =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = JpegRule,
                [".jpeg"] = JpegRule,
                [".png"] = PngRule,
                [".webp"] = WebpRule,
                [".pdf"] = PdfRule
            };

        private static readonly Dictionary<FileUploadPolicy, PolicyRule> PolicyRules = new()
        {
            [FileUploadPolicy.PropertyImage] = new PolicyRule(
                MaxPropertyImageBytes,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" }),
            [FileUploadPolicy.VerificationImage] = new PolicyRule(
                MaxVerificationImageBytes,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" }),
            [FileUploadPolicy.LegalDocument] = new PolicyRule(
                MaxLegalDocumentBytes,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".jpg", ".jpeg", ".png" })
        };

        public async Task ValidateAsync(Stream fileStream, string fileName, string contentType, FileUploadPolicy policy)
        {
            if (fileStream == null || !fileStream.CanRead || fileStream.Length <= 0)
            {
                throw new ValidationException("El archivo está vacío o no es legible.");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ValidationException("El nombre del archivo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(contentType))
            {
                throw new ValidationException("El tipo MIME del archivo es obligatorio.");
            }

            if (!PolicyRules.TryGetValue(policy, out var policyRule))
            {
                throw new ValidationException("Política de validación de archivo no reconocida.");
            }

            if (fileStream.Length > policyRule.MaxSizeBytes)
            {
                var maxMegabytes = policyRule.MaxSizeBytes / (1024 * 1024);
                throw new ValidationException($"El archivo supera el tamaño máximo permitido de {maxMegabytes} MB.");
            }

            var extension = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(extension) ||
                !policyRule.AllowedExtensions.Contains(extension) ||
                !FormatRulesByExtension.TryGetValue(extension, out var formatRule))
            {
                throw new ValidationException($"La extensión de archivo '{extension}' no está permitida.");
            }

            var normalizedMime = contentType.Split(';')[0].Trim();
            if (!formatRule.AllowedMimeTypes.Contains(normalizedMime))
            {
                throw new ValidationException($"El tipo MIME '{normalizedMime}' no corresponde a la extensión '{extension}'.");
            }

            var (header, bytesRead) = await ReadHeaderAsync(fileStream);
            if (!formatRule.MatchesMagicBytes(header, bytesRead))
            {
                throw new ValidationException("La firma binaria del archivo no coincide con el formato declarado.");
            }
        }

        private static async Task<(byte[] Header, int BytesRead)> ReadHeaderAsync(Stream fileStream)
        {
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var header = new byte[HeaderInspectLength];
            var totalRead = 0;
            while (totalRead < HeaderInspectLength)
            {
                var read = await fileStream.ReadAsync(header.AsMemory(totalRead, HeaderInspectLength - totalRead));
                if (read == 0)
                {
                    break;
                }
                totalRead += read;
            }

            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            return (header, totalRead);
        }

        private static bool IsJpeg(byte[] header, int length)
            => length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

        private static bool IsPng(byte[] header, int length)
            => length >= 8 &&
               header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
               header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

        private static bool IsWebp(byte[] header, int length)
        {
            if (length < 4) return false;
            var hasRiff = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46;
            if (!hasRiff) return false;
            if (length < 12) return true;

            return header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
        }

        private static bool IsPdf(byte[] header, int length)
            => length >= 5 &&
               header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 &&
               header[3] == 0x46 && header[4] == 0x2D;
    }
}
