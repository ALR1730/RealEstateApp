using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class PropertyDocumentServiceTests
    {
        private readonly Mock<IPropertyDocumentRepository> _documentRepositoryMock;
        private readonly Mock<IPropertyRepository> _propertyRepositoryMock;
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly PropertyDocumentService _sut;

        public PropertyDocumentServiceTests()
        {
            _documentRepositoryMock = new Mock<IPropertyDocumentRepository>();
            _propertyRepositoryMock = new Mock<IPropertyRepository>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            var store = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _sut = new PropertyDocumentService(
                _documentRepositoryMock.Object,
                _propertyRepositoryMock.Object,
                _fileStorageServiceMock.Object,
                _userManagerMock.Object);
        }

        private static MemoryStream CreateStream(string content = "pdf-content")
            => new(Encoding.UTF8.GetBytes(content));

        [Fact]
        public async Task UploadAsync_AgentePropietario_SubeArchivoYRegistraDocumento()
        {
            // Arrange
            var property = new Property { Id = 5, AgentId = "agent-1", Name = "Casa", Code = "CSA001" };
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);
            _fileStorageServiceMock.Setup(f => f.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), "property-documents"))
                .ReturnsAsync("/uploads/property-documents/doc.pdf");
            _documentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PropertyDocument>()))
                .ReturnsAsync((PropertyDocument d) => { d.Id = 1; return d; });

            using var stream = CreateStream();

            // Act
            var result = await _sut.UploadAsync(5, DocumentTypeConstants.ContratoVenta, "agent-1", stream, "contrato.pdf", "application/pdf");

            // Assert
            _fileStorageServiceMock.Verify(f => f.UploadFileAsync(It.IsAny<Stream>(), "contrato.pdf", "property-documents"), Times.Once);
            _documentRepositoryMock.Verify(r => r.AddAsync(It.Is<PropertyDocument>(d =>
                d.PropertyId == 5 &&
                d.DocumentType == DocumentTypeConstants.ContratoVenta &&
                d.UploadedBy == "agent-1" &&
                d.OriginalFileName == "contrato.pdf")), Times.Once);
            result.FileUrl.Should().Be("/uploads/property-documents/doc.pdf");
        }

        [Fact]
        public async Task UploadAsync_PropiedadInexistente_ThrowsNotFoundException()
        {
            // Arrange
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Property?)null);

            // Act
            Func<Task> act = async () => await _sut.UploadAsync(99, DocumentTypeConstants.Titulo, "agent-1", CreateStream(), "doc.pdf", "application/pdf");

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UploadAsync_NoEsPropietario_ThrowsValidationException()
        {
            // Arrange
            var property = new Property { Id = 5, AgentId = "otro-agente", Name = "Casa" };
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);

            // Act
            Func<Task> act = async () => await _sut.UploadAsync(5, DocumentTypeConstants.Titulo, "agent-1", CreateStream(), "doc.pdf", "application/pdf");

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*tus propias propiedades*");
        }

        [Fact]
        public async Task UploadAsync_TipoInvalido_ThrowsValidationException()
        {
            // Arrange
            var property = new Property { Id = 5, AgentId = "agent-1", Name = "Casa" };
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);

            // Act
            Func<Task> act = async () => await _sut.UploadAsync(5, "Tipo inventado", "agent-1", CreateStream(), "doc.pdf", "application/pdf");

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Tipo de documento inválido*");
        }

        [Fact]
        public async Task GetByPropertyIdAsync_EsPropietario_RetornaDocumentos()
        {
            // Arrange
            var property = new Property { Id = 5, AgentId = "agent-1", Name = "Casa", Code = "CSA001" };
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);
            _documentRepositoryMock.Setup(r => r.GetByPropertyIdAsync(5))
                .ReturnsAsync(new List<PropertyDocument>
                {
                    new PropertyDocument { Id = 1, PropertyId = 5, Property = property, DocumentType = DocumentTypeConstants.Titulo, OriginalFileName = "titulo.pdf" }
                });

            // Act
            var result = await _sut.GetByPropertyIdAsync(5, "agent-1", isAdmin: false);

            // Assert
            var doc = result.Should().ContainSingle().Subject;
            doc.PropertyCode.Should().Be("CSA001");
            doc.PropertyName.Should().Be("Casa");
            doc.DocumentType.Should().Be(DocumentTypeConstants.Titulo);
        }

        [Fact]
        public async Task GetByPropertyIdAsync_NoEsPropietario_ThrowsValidationException()
        {
            // Arrange
            var property = new Property { Id = 5, AgentId = "otro-agente", Name = "Casa" };
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);

            // Act
            Func<Task> act = async () => await _sut.GetByPropertyIdAsync(5, "agent-1", isAdmin: false);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No tiene permisos*");
        }

        [Fact]
        public async Task GetByPropertyIdAsync_Administrador_NoAplicaChequeoDePropiedad()
        {
            // Arrange
            var property = new Property { Id = 5, AgentId = "otro-agente", Name = "Casa" };
            _propertyRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(property);
            _documentRepositoryMock.Setup(r => r.GetByPropertyIdAsync(5))
                .ReturnsAsync(new List<PropertyDocument>());

            // Act
            var result = await _sut.GetByPropertyIdAsync(5, "admin-1", isAdmin: true);

            // Assert
            result.Should().BeEmpty();
        }
    }
}