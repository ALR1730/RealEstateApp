using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Entities;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class PropertyImageServiceTests
    {
        private readonly Mock<IPropertyImageRepository> _imageRepoMock;
        private readonly Mock<IFileStorageService> _storageMock;
        private readonly PropertyImageService _sut;

        // Firmas mágicas reales para simular archivos válidos
        private static readonly byte[] JpegHeader = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        public PropertyImageServiceTests()
        {
            _imageRepoMock = new Mock<IPropertyImageRepository>();
            _storageMock = new Mock<IFileStorageService>();
            _sut = new PropertyImageService(_imageRepoMock.Object, _storageMock.Object);
        }

        /// <summary>
        /// Crea un mock de IFormFile con nombre, MIME type y contenido binario específicos.
        /// </summary>
        private static Mock<IFormFile> CreateFileMock(string fileName, string contentType, byte[] content)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(content.Length);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(content));
            return fileMock;
        }

        [Fact]
        public async Task SaveImagesAsync_DebeSubirYGuardarEntidades_RespetandoLimiteDeSlots()
        {
            // Arrange — usa header JPEG real para pasar las 3 guardias de seguridad
            var existingImages = new List<PropertyImage>
            {
                new PropertyImage { Id = 1, PropertyId = 10, ImageUrl = "/uploads/img1.jpg" }
            };

            _imageRepoMock.Setup(r => r.GetByPropertyIdAsync(10))
                .ReturnsAsync(existingImages);

            _storageMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "properties"))
                .ReturnsAsync("/uploads/img2.jpg");

            var fileMock = CreateFileMock("photo.jpg", "image/jpeg", JpegHeader);
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var saved = await _sut.SaveImagesAsync(10, files, maxImages: 15);

            // Assert
            saved.Should().HaveCount(1);
            _storageMock.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), "photo.jpg", "properties"), Times.Once);
            _imageRepoMock.Verify(r => r.AddRangeAsync(It.IsAny<List<PropertyImage>>()), Times.Once);
        }

        [Fact]
        public async Task DeletePhysicalImagesAsync_DebeInvocarStorageParaCadaImagen()
        {
            // Arrange
            var images = new List<PropertyImage>
            {
                new PropertyImage { Id = 1, ImageUrl = "/uploads/1.jpg" },
                new PropertyImage { Id = 2, ImageUrl = "/uploads/2.jpg" }
            };

            // Act
            await _sut.DeletePhysicalImagesAsync(images);

            // Assert
            _storageMock.Verify(s => s.DeleteFileAsync("/uploads/1.jpg", "properties"), Times.Once);
            _storageMock.Verify(s => s.DeleteFileAsync("/uploads/2.jpg", "properties"), Times.Once);
        }

        #region Fase 2 — Seguridad OWASP: Whitelist MIME + Magic Bytes (Hallazgo 2.3)

        /// <summary>
        /// Un archivo con MIME type no permitido debe ser rechazado sin llegar al storage.
        /// </summary>
        [Fact]
        public async Task SaveImagesAsync_DebeRechazarArchivo_CuandoMimeTypeNoEstaEnWhitelist()
        {
            // Arrange — PDF con MIME no permitido
            var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            _imageRepoMock.Setup(r => r.GetByPropertyIdAsync(1)).ReturnsAsync(new List<PropertyImage>());

            var fileMock = CreateFileMock("factura.pdf", "application/pdf", pdfHeader);
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var saved = await _sut.SaveImagesAsync(1, files);

            // Assert
            saved.Should().BeEmpty("los archivos con MIME type prohibido deben ser rechazados antes del upload");
            _storageMock.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Un ejecutable renombrado con extensión .jpg y MIME image/jpeg debe ser bloqueado
        /// por la guardia de firma mágica (magic bytes). Previene upload de malware.
        /// </summary>
        [Fact]
        public async Task SaveImagesAsync_DebeRechazarArchivo_CuandoFirmaMagicaNoCoincidesConImagen()
        {
            // Arrange — header MZ (Windows PE executable) con MIME declarado como JPEG
            var exeHeader = new byte[] { 0x4D, 0x5A, 0x90, 0x00 }; // MZ header
            _imageRepoMock.Setup(r => r.GetByPropertyIdAsync(2)).ReturnsAsync(new List<PropertyImage>());

            var fileMock = CreateFileMock("malware.jpg", "image/jpeg", exeHeader);
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var saved = await _sut.SaveImagesAsync(2, files);

            // Assert
            saved.Should().BeEmpty("un ejecutable renombrado debe ser bloqueado por la firma binaria aunque el MIME sea válido");
            _storageMock.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Archivos PNG legítimos deben pasar las 3 guardias y ser aceptados correctamente.
        /// Test de regresión: la whitelist no debe bloquear formatos permitidos.
        /// </summary>
        [Fact]
        public async Task SaveImagesAsync_DebeAceptarArchivo_CuandoPngEsValido()
        {
            // Arrange
            _imageRepoMock.Setup(r => r.GetByPropertyIdAsync(3)).ReturnsAsync(new List<PropertyImage>());
            _storageMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "properties"))
                .ReturnsAsync("/uploads/plano.png");

            var fileMock = CreateFileMock("plano.png", "image/png", PngHeader);
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var saved = await _sut.SaveImagesAsync(3, files);

            // Assert
            saved.Should().HaveCount(1, "un PNG válido debe ser aceptado y subido al storage");
            _storageMock.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "properties"), Times.Once);
        }

        /// <summary>
        /// Archivos que superen el límite de 10 MB deben ser rechazados.
        /// Previene ataques de agotamiento de almacenamiento (storage exhaustion).
        /// </summary>
        [Fact]
        public async Task SaveImagesAsync_DebeRechazarArchivo_CuandoSupera10MB()
        {
            // Arrange — simular archivo de 11 MB sin llegar a crear 11MB de bytes reales
            const long oneMB = 1024 * 1024;
            _imageRepoMock.Setup(r => r.GetByPropertyIdAsync(4)).ReturnsAsync(new List<PropertyImage>());

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(11 * oneMB);
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.FileName).Returns("foto_gigante.jpg");
            fileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(JpegHeader));

            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var saved = await _sut.SaveImagesAsync(4, files);

            // Assert
            saved.Should().BeEmpty("archivos mayores a 10 MB deben ser rechazados antes del upload");
            _storageMock.Verify(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
