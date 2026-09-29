using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public PropertyImageServiceTests()
        {
            _imageRepoMock = new Mock<IPropertyImageRepository>();
            _storageMock = new Mock<IFileStorageService>();
            _sut = new PropertyImageService(_imageRepoMock.Object, _storageMock.Object);
        }

        [Fact]
        public async Task SaveImagesAsync_DebeSubirYGuardarEntidades_RespetandoLimiteDeSlots()
        {
            // Arrange
            var existingImages = new List<PropertyImage>
            {
                new PropertyImage { Id = 1, PropertyId = 10, ImageUrl = "/uploads/img1.jpg" }
            };

            _imageRepoMock.Setup(r => r.GetByPropertyIdAsync(10))
                .ReturnsAsync(existingImages);

            _storageMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "properties"))
                .ReturnsAsync("/uploads/img2.jpg");

            var fileMock = new Mock<IFormFile>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake-image"));
            fileMock.Setup(f => f.Length).Returns(stream.Length);
            fileMock.Setup(f => f.FileName).Returns("photo.jpg");
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

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
    }
}
