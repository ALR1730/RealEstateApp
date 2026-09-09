using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Domain.Entities
{
    /// <summary>
    /// Documento legal/contractual asociado a una propiedad (Ítem 2.6).
    /// El agente sube la documentación; el agente y el administrador pueden consultarla.
    /// </summary>
    public class PropertyDocument : AuditableBaseEntity
    {
        public int PropertyId { get; set; }
        public string DocumentType { get; set; } = DocumentTypeConstants.Titulo;
        public string FileUrl { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public long SizeBytes { get; set; }
        public string UploadedBy { get; set; } = string.Empty;

        public Property? Property { get; set; }
    }
}