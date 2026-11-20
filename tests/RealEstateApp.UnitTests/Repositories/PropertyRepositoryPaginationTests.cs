using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.DTOs.Common;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using RealEstateApp.Infrastructure.Persistence.Repositories;
using Xunit;

namespace RealEstateApp.UnitTests.Repositories
{
    public class PropertyRepositoryPaginationTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static async Task<(int PropertyTypeId, int SaleTypeId)> SeedCatalogsAndPropertiesAsync(
            ApplicationDbContext dbContext,
            int totalProperties,
            int soldCount = 0)
        {
            var propertyType = new PropertyType { Name = "Apartamento", Description = "Apto" };
            var saleType = new SaleType { Name = "Venta", Description = "Venta directa" };
            await dbContext.PropertyTypes.AddAsync(propertyType);
            await dbContext.SaleTypes.AddAsync(saleType);
            await dbContext.SaveChangesAsync();

            var properties = new List<Property>();
            for (int i = 1; i <= totalProperties; i++)
            {
                var property = new Property
                {
                    Name = $"Propiedad {i:D3}",
                    Code = $"P{i:D5}",
                    Price = 5_000_000m + (i * 10_000m),
                    PriceInDOP = 5_000_000m + (i * 10_000m),
                    Currency = CurrencyConstants.DOP,
                    Rooms = 3,
                    Bathrooms = 2,
                    SizeInMeters = 120m,
                    AgentId = "agent-1",
                    PropertyTypeId = propertyType.Id,
                    SaleTypeId = saleType.Id
                };

                if (i <= soldCount)
                {
                    property.MarkAsSold();
                }

                properties.Add(property);
            }

            await dbContext.Properties.AddRangeAsync(properties);
            await dbContext.SaveChangesAsync();

            return (propertyType.Id, saleType.Id);
        }

        [Fact]
        public async Task GetWithFiltersPagedAsync_DebeLimitarPageSizeA50_CuandoSeSolicitan200()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using (var seedCtx = CreateContext(dbName))
            {
                await SeedCatalogsAndPropertiesAsync(seedCtx, 65);
            }

            await using var readCtx = CreateContext(dbName);
            var repo = new PropertyRepository(readCtx);
            var filter = new PropertyFilterViewModel { Page = 1, PageSize = 200 };

            // Act
            var result = await repo.GetWithFiltersPagedAsync(filter);

            // Assert
            result.Items.Should().HaveCount(50);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(PagedResult<Property>.MaxPageSize);
            result.TotalCount.Should().Be(65);
            result.TotalPages.Should().Be(2);
            result.HasPreviousPage.Should().BeFalse();
            result.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetWithFiltersPagedAsync_DebeUsarValoresPorDefecto_CuandoNoSeEnvianParametros()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using (var seedCtx = CreateContext(dbName))
            {
                await SeedCatalogsAndPropertiesAsync(seedCtx, 30);
            }

            await using var readCtx = CreateContext(dbName);
            var repo = new PropertyRepository(readCtx);

            // Act
            var result = await repo.GetWithFiltersPagedAsync(new PropertyFilterViewModel());

            // Assert
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(PagedResult<Property>.DefaultPageSize);
            result.Items.Should().HaveCount(20);
            result.TotalCount.Should().Be(30);
            result.TotalPages.Should().Be(2);
            result.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetWithFiltersPagedAsync_DebeExcluirVendidas_CuandoIncludeSoldEsFalse()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using (var seedCtx = CreateContext(dbName))
            {
                await SeedCatalogsAndPropertiesAsync(seedCtx, totalProperties: 12, soldCount: 4);
            }

            await using var readCtx = CreateContext(dbName);
            var repo = new PropertyRepository(readCtx);

            // Act
            var publicPage = await repo.GetWithFiltersPagedAsync(new PropertyFilterViewModel(), includeSold: false);

            // Assert
            publicPage.TotalCount.Should().Be(8);
            publicPage.Items.Should().HaveCount(8);
            publicPage.Items.Should().OnlyContain(p => p.Status != PropertyStatus.Sold);
        }

        [Fact]
        public async Task GetPagedAsync_DebeRetornarSegundaPaginaYMetadatosCorrectos()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using (var seedCtx = CreateContext(dbName))
            {
                await SeedCatalogsAndPropertiesAsync(seedCtx, 25);
            }

            await using var readCtx = CreateContext(dbName);
            var repo = new PropertyRepository(readCtx);

            // Act
            var page2 = await repo.GetPagedAsync(page: 2, pageSize: 10);

            // Assert
            page2.Page.Should().Be(2);
            page2.PageSize.Should().Be(10);
            page2.TotalCount.Should().Be(25);
            page2.TotalPages.Should().Be(3);
            page2.Items.Should().HaveCount(10);
            page2.HasPreviousPage.Should().BeTrue();
            page2.HasNextPage.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_DebeRespetarLimiteMaximoDe50Registros()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            await using (var seedCtx = CreateContext(dbName))
            {
                await SeedCatalogsAndPropertiesAsync(seedCtx, 65);
            }

            await using var readCtx = CreateContext(dbName);
            var repo = new PropertyRepository(readCtx);

            // Act
            var results = await repo.GetAllAsync();

            // Assert
            results.Should().HaveCount(PagedResult<Property>.MaxPageSize);
        }

        [Fact]
        public async Task GetByIdAsync_NoDebeCargarOffersNiChats_EnConsultaPublicaPorId()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            int propertyId;

            await using (var seedCtx = CreateContext(dbName))
            {
                var (propertyTypeId, saleTypeId) = await SeedCatalogsAndPropertiesAsync(seedCtx, 1);
                var property = await seedCtx.Properties.FirstAsync();
                propertyId = property.Id;

                await seedCtx.PropertyImages.AddAsync(new PropertyImage
                {
                    PropertyId = propertyId,
                    ImageUrl = "/images/properties/sample.jpg"
                });

                await seedCtx.Offers.AddAsync(new Offer
                {
                    PropertyId = propertyId,
                    ClienteId = "client-secret",
                    MontoOfertado = 4_900_000m,
                    FechaOferta = DateTime.UtcNow
                });

                await seedCtx.Chats.AddAsync(new Chat
                {
                    PropertyId = propertyId,
                    ClienteId = "client-secret",
                    AgenteId = "agent-1",
                    SenderId = "client-secret",
                    MessageContent = "Mensaje privado de negociación",
                    SentAt = DateTime.UtcNow
                });

                await seedCtx.SaveChangesAsync();
            }

            await using var readCtx = CreateContext(dbName);
            var repo = new PropertyRepository(readCtx);

            // Act
            var loaded = await repo.GetByIdAsync(propertyId);

            // Assert
            loaded.Should().NotBeNull();
            loaded!.PropertyType.Should().NotBeNull();
            loaded.SaleType.Should().NotBeNull();
            loaded.Images.Should().HaveCount(1);
            loaded.Offers.Should().BeNullOrEmpty("la consulta pública por ID no debe cargar ofertas privadas");
            loaded.Chats.Should().BeNullOrEmpty("la consulta pública por ID no debe cargar hilos privados de chat");
        }
    }
}
