using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UnitOfWork _sut;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _sut = new UnitOfWork(_dbContext);
        }

        [Fact]
        public async Task ExecuteTransactionAsync_DebeEjecutarAccionYPersistirCambios_CuandoAccionEsExitosa()
        {
            // Arrange
            var ejecutado = false;
            var propertyType = new PropertyType { Name = "Penthouse", Description = "Lujoso" };

            // Act
            await _sut.ExecuteTransactionAsync(async () =>
            {
                await _dbContext.PropertyTypes.AddAsync(propertyType);
                ejecutado = true;
            });

            // Assert
            ejecutado.Should().BeTrue();
            var saved = await _dbContext.PropertyTypes.FirstOrDefaultAsync(p => p.Name == "Penthouse");
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("Penthouse");
        }

        [Fact]
        public async Task ExecuteTransactionAsync_DebeRetornarValor_CuandoSeEjecutaSobrecargaGenerica()
        {
            // Arrange & Act
            var resultado = await _sut.ExecuteTransactionAsync(async () =>
            {
                await Task.Yield();
                return 100;
            });

            // Assert
            resultado.Should().Be(100);
        }

        [Fact]
        public async Task ExecuteTransactionAsync_DebePropagarExcepcion_CuandoAccionFalla()
        {
            // Arrange
            Func<Task> act = async () =>
            {
                await _sut.ExecuteTransactionAsync(async () =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("Fallo intencional para verificar atomicidad");
                });
            };

            // Act & Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Fallo intencional para verificar atomicidad");
        }

        [Fact]
        public async Task CommitAsync_DebePersistirCambiosPendientes_YRetornarConteoDeEntidades()
        {
            // Arrange
            var saleType = new SaleType { Name = "Alquiler", Description = "Renta mensual" };
            await _dbContext.SaleTypes.AddAsync(saleType);

            // Act
            var filasAfectadas = await _sut.CommitAsync();

            // Assert
            filasAfectadas.Should().BeGreaterThan(0);
            var saved = await _dbContext.SaleTypes.FirstOrDefaultAsync(s => s.Name == "Alquiler");
            saved.Should().NotBeNull();
        }

        public void Dispose()
        {
            _sut.Dispose();
            _dbContext.Dispose();
        }
    }
}
