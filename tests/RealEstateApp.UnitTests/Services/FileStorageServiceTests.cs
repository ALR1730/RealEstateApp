using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Infrastructure.Shared.Services;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class FileStorageServiceTests : IDisposable
    {
        private readonly string _testRoot;
        private readonly string _basePath;
        private readonly string _canaryOutsideFile;
        private readonly FileStorageService _sut;

        public FileStorageServiceTests()
        {
            _testRoot = Path.Combine(Path.GetTempPath(), $"FileStorageTests_{Guid.NewGuid()}");
            _basePath = Path.Combine(_testRoot, "uploads");
            Directory.CreateDirectory(_basePath);

            _canaryOutsideFile = Path.Combine(_testRoot, "secret.txt");
            File.WriteAllText(_canaryOutsideFile, "TOP_SECRET_CONFIG");

            _sut = new FileStorageService(_basePath);
        }

        [Theory]
        [InlineData("/../../appsettings.json")]
        [InlineData("..\\..\\secret.txt")]
        [InlineData("../secret.txt")]
        [InlineData("properties/../../secret.txt")]
        public async Task DeleteFileAsync_DebeRechazarPathTraversal_SinEliminarArchivosFueraDeBasePath(string maliciousUrl)
        {
            // Arrange & Act
            Func<Task> act = () => _sut.DeleteFileAsync(maliciousUrl, "properties");

            // Assert
            var exception = await act.Should().ThrowAsync<Exception>();
            exception.Which.Should().Match(ex =>
                ex is ValidationException || ex is UnauthorizedAccessException);

            File.Exists(_canaryOutsideFile).Should().BeTrue(
                "un intento de path traversal jamás debe eliminar archivos fuera de _basePath");
        }

        [Theory]
        [InlineData("../escape")]
        [InlineData("..\\escape")]
        [InlineData("sub/folder")]
        [InlineData("sub\\folder")]
        [InlineData("..")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UploadFileAsync_DebeRechazarContainerNameInvalido(string invalidContainer)
        {
            // Arrange
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("contenido-prueba"));

            // Act
            Func<Task> act = () => _sut.UploadFileAsync(stream, "imagen.jpg", invalidContainer);

            // Assert
            var exception = await act.Should().ThrowAsync<Exception>();
            exception.Which.Should().Match(ex =>
                ex is ValidationException || ex is UnauthorizedAccessException);

            Directory.GetFiles(_basePath, "*", SearchOption.AllDirectories)
                .Should().BeEmpty("no debe guardarse ningún archivo si containerName es inválido");
        }

        [Theory]
        [InlineData("../escape")]
        [InlineData("..\\escape")]
        [InlineData("sub/folder")]
        [InlineData("sub\\folder")]
        [InlineData("..")]
        [InlineData("")]
        public async Task DeleteFileAsync_DebeRechazarContainerNameInvalido(string invalidContainer)
        {
            // Arrange & Act
            Func<Task> act = () => _sut.DeleteFileAsync("archivo.jpg", invalidContainer);

            // Assert
            var exception = await act.Should().ThrowAsync<Exception>();
            exception.Which.Should().Match(ex =>
                ex is ValidationException || ex is UnauthorizedAccessException);
        }

        [Fact]
        public async Task UploadFileAsyncYDeleteFileAsync_DebenFuncionarCorrectamenteDentroDeBasePath()
        {
            // Arrange
            var payload = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x01, 0x02 };
            using var stream = new MemoryStream(payload);
            const string containerName = "properties";

            // Act - Upload válido
            var uploadedUrl = await _sut.UploadFileAsync(stream, "fachada.jpg", containerName);

            // Assert - Archivo creado dentro de _basePath
            uploadedUrl.Should().NotBeNullOrWhiteSpace();
            var filesAfterUpload = Directory.GetFiles(_basePath, "*", SearchOption.AllDirectories);
            filesAfterUpload.Should().ContainSingle();
            Path.GetFullPath(filesAfterUpload[0]).StartsWith(
                Path.GetFullPath(_basePath),
                StringComparison.OrdinalIgnoreCase).Should().BeTrue();

            // Act - Delete válido
            Func<Task> deleteAct = () => _sut.DeleteFileAsync(uploadedUrl, containerName);

            // Assert - Archivo eliminado limpiamente
            await deleteAct.Should().NotThrowAsync();
            Directory.GetFiles(_basePath, "*", SearchOption.AllDirectories).Should().BeEmpty();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testRoot))
            {
                Directory.Delete(_testRoot, recursive: true);
            }
        }
    }
}
