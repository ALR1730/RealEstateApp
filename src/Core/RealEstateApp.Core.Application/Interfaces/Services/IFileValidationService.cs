using System.IO;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public enum FileUploadPolicy
    {
        PropertyImage,
        VerificationImage,
        LegalDocument
    }

    public interface IFileValidationService
    {
        Task ValidateAsync(Stream fileStream, string fileName, string contentType, FileUploadPolicy policy);
    }
}
