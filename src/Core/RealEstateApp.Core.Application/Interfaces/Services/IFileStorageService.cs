using System.IO;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
        Task DeleteFileAsync(string fileUrl, string containerName);
    }
}
