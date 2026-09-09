using System;

namespace RealEstateApp.Core.Application.ViewModels.Document
{
    public class PropertyDocumentViewModel
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string? PropertyCode { get; set; }
        public string? PropertyName { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public string? UploadedByName { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool IsImage =>
            ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}