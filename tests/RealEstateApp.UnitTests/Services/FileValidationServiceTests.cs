using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Domain.Exceptions;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class FileValidationServiceTests
    {
        private const long FiveMegabytes = 5L * 1024 * 1024;
        private const long TenMegabytes = 10L * 1024 * 1024;

        private static readonly byte[] PdfHeader = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A }; // %PDF-1.4\n
        private static readonly byte[] JpegHeader = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01 };
        private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D };
        private static readonly byte[] WebpHeader = { 0x52, 0x49, 0x46, 0x46, 0x24, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38, 0x20 };
        private static readonly byte[] ExecutableMzHeader = { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };

        private readonly IFileValidationService _sut;

        public FileValidationServiceTests()
        {
            _sut = new FileValidationService();
        }

        #region Casos Válidos por Política

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage)]
        [InlineData(FileUploadPolicy.VerificationImage)]
        [InlineData(FileUploadPolicy.LegalDocument)]
        public async Task ValidateAsync_DebeAceptarJpegValido_ParaTodasLasPoliticas(FileUploadPolicy policy)
        {
            // Arrange
            using var stream = new MemoryStream(JpegHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "propiedad.jpg", "image/jpeg", policy);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage)]
        [InlineData(FileUploadPolicy.VerificationImage)]
        [InlineData(FileUploadPolicy.LegalDocument)]
        public async Task ValidateAsync_DebeAceptarPngValido_ParaTodasLasPoliticas(FileUploadPolicy policy)
        {
            // Arrange
            using var stream = new MemoryStream(PngHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "captura.PNG", "image/png", policy);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage)]
        [InlineData(FileUploadPolicy.VerificationImage)]
        public async Task ValidateAsync_DebeAceptarWebpValido_ParaPoliticasDeImagen(FileUploadPolicy policy)
        {
            // Arrange
            using var stream = new MemoryStream(WebpHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "fachada.webp", "image/webp", policy);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ValidateAsync_DebeAceptarPdfValido_CuandoPoliticaEsLegalDocument()
        {
            // Arrange
            using var stream = new MemoryStream(PdfHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "titulo_propiedad.pdf", "application/pdf", FileUploadPolicy.LegalDocument);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ValidateAsync_DebeAceptarArchivoEnElLimiteExactoDe5MB_ParaVerificationImage()
        {
            // Arrange
            using var stream = new BoundedFakeStream(JpegHeader, FiveMegabytes);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "cedula.jpeg", "image/jpeg", FileUploadPolicy.VerificationImage);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage, "foto.jpg", "image/jpeg")]
        [InlineData(FileUploadPolicy.LegalDocument, "escritura.pdf", "application/pdf")]
        public async Task ValidateAsync_DebeAceptarArchivoEnElLimiteExactoDe10MB_ParaPropertyImageYLegalDocument(
            FileUploadPolicy policy,
            string fileName,
            string contentType)
        {
            // Arrange
            var header = policy == FileUploadPolicy.LegalDocument ? PdfHeader : JpegHeader;
            using var stream = new BoundedFakeStream(header, TenMegabytes);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, fileName, contentType, policy);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion

        #region Rechazo de Extensiones Peligrosas, Magic Bytes Falsos y MIME Spoofing

        [Theory]
        [InlineData("payload.html", "text/html", "<html><script>alert(1)</script></html>", FileUploadPolicy.PropertyImage)]
        [InlineData("payload.html", "text/html", "<html><script>alert(1)</script></html>", FileUploadPolicy.VerificationImage)]
        [InlineData("payload.html", "text/html", "<html><script>alert(1)</script></html>", FileUploadPolicy.LegalDocument)]
        [InlineData("vector.svg", "image/svg+xml", "<svg onload=\"alert(1)\"></svg>", FileUploadPolicy.PropertyImage)]
        [InlineData("vector.svg", "image/svg+xml", "<svg onload=\"alert(1)\"></svg>", FileUploadPolicy.VerificationImage)]
        [InlineData("vector.svg", "image/svg+xml", "<svg onload=\"alert(1)\"></svg>", FileUploadPolicy.LegalDocument)]
        [InlineData("script.js", "application/javascript", "alert('xss');", FileUploadPolicy.PropertyImage)]
        [InlineData("script.js", "application/javascript", "alert('xss');", FileUploadPolicy.VerificationImage)]
        [InlineData("script.js", "application/javascript", "alert('xss');", FileUploadPolicy.LegalDocument)]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoExtensionEsPeligrosa(
            string fileName,
            string contentType,
            string content,
            FileUploadPolicy policy)
        {
            // Arrange
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, fileName, contentType, policy);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage)]
        [InlineData(FileUploadPolicy.VerificationImage)]
        [InlineData(FileUploadPolicy.LegalDocument)]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoEjecutableMzEsRenombradoAJpg(FileUploadPolicy policy)
        {
            // Arrange
            using var stream = new MemoryStream(ExecutableMzHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "troyano.jpg", "image/jpeg", policy);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoPdfFalsificadoNoIniciaConMagicBytesPdf()
        {
            // Arrange
            var fakePdfBytes = Encoding.UTF8.GetBytes("<html><body>No soy un PDF real</body></html>");
            using var stream = new MemoryStream(fakePdfBytes);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "contrato.pdf", "application/pdf", FileUploadPolicy.LegalDocument);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoHtmlSeRenombraAJpg()
        {
            // Arrange
            var htmlBytes = Encoding.UTF8.GetBytes("<!DOCTYPE html><script>alert(1)</script>");
            using var stream = new MemoryStream(htmlBytes);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "foto.jpg", "image/jpeg", FileUploadPolicy.VerificationImage);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage, "foto.jpg", "text/html")]
        [InlineData(FileUploadPolicy.PropertyImage, "foto.jpg", "image/svg+xml")]
        [InlineData(FileUploadPolicy.VerificationImage, "kyc.png", "application/x-msdownload")]
        [InlineData(FileUploadPolicy.LegalDocument, "doc.pdf", "image/jpeg")]
        [InlineData(FileUploadPolicy.LegalDocument, "doc.pdf", "text/html")]
        public async Task ValidateAsync_DebeLanzarValidationException_AnteMimeSpoofing(
            FileUploadPolicy policy,
            string fileName,
            string spoofedMime)
        {
            // Arrange
            var header = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? PdfHeader
                : (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? PngHeader : JpegHeader);
            using var stream = new MemoryStream(header);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, fileName, spoofedMime, policy);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage)]
        [InlineData(FileUploadPolicy.VerificationImage)]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoSeSubePdfEnPoliticaSoloDeImagenes(FileUploadPolicy policy)
        {
            // Arrange
            using var stream = new MemoryStream(PdfHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "documento.pdf", "application/pdf", policy);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoSeSubeWebpEnPoliticaLegalDocument()
        {
            // Arrange
            using var stream = new MemoryStream(WebpHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, "imagen.webp", "image/webp", FileUploadPolicy.LegalDocument);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        #endregion

        #region Rechazo por Tamaño, Vacío y Entradas Nulas

        [Theory]
        [InlineData(FileUploadPolicy.PropertyImage, "foto.jpg", "image/jpeg")]
        [InlineData(FileUploadPolicy.VerificationImage, "kyc.png", "image/png")]
        [InlineData(FileUploadPolicy.LegalDocument, "escritura.pdf", "application/pdf")]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoArchivoEstaVacio(
            FileUploadPolicy policy,
            string fileName,
            string contentType)
        {
            // Arrange
            using var emptyStream = new MemoryStream(Array.Empty<byte>());

            // Act
            Func<Task> act = () => _sut.ValidateAsync(emptyStream, fileName, contentType, policy);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoVerificationImageExcede5MB()
        {
            // Arrange
            using var oversizedStream = new BoundedFakeStream(JpegHeader, FiveMegabytes + 1);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(oversizedStream, "selfie.jpg", "image/jpeg", FileUploadPolicy.VerificationImage);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoPropertyImageExcede10MB()
        {
            // Arrange
            using var oversizedStream = new BoundedFakeStream(PngHeader, TenMegabytes + 1);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(oversizedStream, "fachada.png", "image/png", FileUploadPolicy.PropertyImage);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoLegalDocumentExcede10MB()
        {
            // Arrange
            using var oversizedStream = new BoundedFakeStream(PdfHeader, TenMegabytes + 1);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(oversizedStream, "titulo.pdf", "application/pdf", FileUploadPolicy.LegalDocument);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoNombreDeArchivoEsNuloOVacio(string? invalidFileName)
        {
            // Arrange
            using var stream = new MemoryStream(JpegHeader);

            // Act
            Func<Task> act = () => _sut.ValidateAsync(stream, invalidFileName!, "image/jpeg", FileUploadPolicy.PropertyImage);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ValidateAsync_DebeLanzarValidationException_CuandoStreamEsNulo()
        {
            // Arrange & Act
            Func<Task> act = () => _sut.ValidateAsync(null!, "foto.jpg", "image/jpeg", FileUploadPolicy.PropertyImage);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        #endregion

        /// <summary>
        /// Stream simulado de longitud arbitraria sin asignar buffers de varios megabytes en RAM.
        /// Expone magic bytes reales al inicio y ceros para el resto del payload.
        /// </summary>
        private sealed class BoundedFakeStream : Stream
        {
            private readonly byte[] _header;
            private readonly long _length;
            private long _position;

            public BoundedFakeStream(byte[] header, long length)
            {
                _header = header;
                _length = length;
            }

            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => _length;

            public override long Position
            {
                get => _position;
                set => _position = Math.Clamp(value, 0, _length);
            }

            public override void Flush() { }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_position >= _length || count <= 0)
                {
                    return 0;
                }

                var bytesToRead = (int)Math.Min(count, _length - _position);
                Array.Clear(buffer, offset, bytesToRead);

                for (var i = 0; i < bytesToRead && (_position + i) < _header.Length; i++)
                {
                    buffer[offset + i] = _header[_position + i];
                }

                _position += bytesToRead;
                return bytesToRead;
            }

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                if (_position >= _length || buffer.IsEmpty)
                {
                    return ValueTask.FromResult(0);
                }

                var bytesToRead = (int)Math.Min(buffer.Length, _length - _position);
                var slice = buffer.Span.Slice(0, bytesToRead);
                slice.Clear();

                for (var i = 0; i < bytesToRead && (_position + i) < _header.Length; i++)
                {
                    slice[i] = _header[_position + i];
                }

                _position += bytesToRead;
                return ValueTask.FromResult(bytesToRead);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                var target = origin switch
                {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => _position + offset,
                    SeekOrigin.End => _length + offset,
                    _ => throw new ArgumentOutOfRangeException(nameof(origin))
                };

                _position = Math.Clamp(target, 0, _length);
                return _position;
            }

            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
