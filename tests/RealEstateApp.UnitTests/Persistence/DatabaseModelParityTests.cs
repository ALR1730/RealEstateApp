using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Configuration;
using RealEstateApp.Infrastructure.Persistence.Contexts;
using Xunit;

namespace RealEstateApp.UnitTests.Persistence
{
    public class DatabaseModelParityTests
    {
        [Fact]
        public void ApplicationDbContext_NoDebeTenerCambiosDeModeloPendientes_EnSqlServer()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=ModelCheckOnly;Trusted_Connection=True;",
                    m => m.UseNetTopologySuite())
                .Options;

            using var context = new ApplicationDbContext(options);
            var migrationsAssembly = context.GetService<IMigrationsAssembly>();
            var modelDiffer = context.GetService<IMigrationsModelDiffer>();
            var designTimeModel = context.GetService<IDesignTimeModel>();
            var runtimeInitializer = context.GetService<IModelRuntimeInitializer>();

            var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;
            snapshotModel.Should().NotBeNull("debe existir ApplicationDbContextModelSnapshot compilado");

            if (snapshotModel is IMutableModel mutableModel)
            {
                snapshotModel = mutableModel.FinalizeModel();
            }

            snapshotModel = runtimeInitializer.Initialize(snapshotModel!, designTime: true, validationLogger: null);

            // Act
            var differences = modelDiffer.GetDifferences(
                snapshotModel.GetRelationalModel(),
                designTimeModel.Model.GetRelationalModel());

            // Assert
            differences.Should().BeEmpty(
                $"no deben quedar cambios de modelo pendientes sin migración. Diferencias: {DescribeOperations(differences)}");
        }

        [Fact]
        public void ApplicationDbContext_DebeConfigurarTokensDeConcurrenciaYCompilarModelo_EnPostgreSQL()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql("Host=localhost;Database=ModelCheckOnly;Username=test;Password=test")
                .Options;

            using var context = new ApplicationDbContext(options);

            // Act
            var propertyRowVersion = context.Model
                .FindEntityType(typeof(Property))?
                .FindProperty(nameof(Property.RowVersion));

            var offerRowVersion = context.Model
                .FindEntityType(typeof(Offer))?
                .FindProperty(nameof(Offer.RowVersion));

            // Assert
            propertyRowVersion.Should().NotBeNull();
            propertyRowVersion!.IsConcurrencyToken.Should().BeTrue();
            propertyRowVersion.ValueGenerated.Should().Be(ValueGenerated.Never);

            offerRowVersion.Should().NotBeNull();
            offerRowVersion!.IsConcurrencyToken.Should().BeTrue();
            offerRowVersion.ValueGenerated.Should().Be(ValueGenerated.Never);
        }

        [Fact]
        public void PostgresConnectionStringParser_NoDebeInyectarTrustServerCertificate_PorDefecto()
        {
            // Arrange
            const string url = "postgresql://realestate_user:SecretPass@db-host.render.com:5432/realestate_db";

            // Act
            var parsed = PostgresConnectionStringParser.Parse(url);

            // Assert
            parsed.Should().Contain("Host=db-host.render.com");
            parsed.Should().Contain("Port=5432");
            parsed.Should().Contain("Database=realestate_db");
            parsed.Should().Contain("Username=realestate_user");
            parsed.Should().Contain("Password=SecretPass");
            parsed.Should().Contain("SSL Mode=Require");
            parsed.Should().NotContainEquivalentOf("Trust Server Certificate");
        }

        [Fact]
        public void PostgresConnectionStringParser_DebeRespetarTrustServerCertificate_CuandoSeIndicaEnUrlOConfiguracion()
        {
            // Arrange
            const string urlWithoutQuery = "postgres://user:pass@localhost:5432/appdb";
            const string urlWithQuery = "postgresql://user:pass@db.example.com:5433/appdb?sslmode=VerifyFull&trustservercertificate=true";

            // Act
            var fromConfigFlag = PostgresConnectionStringParser.Parse(urlWithoutQuery, trustServerCertificate: true);
            var fromQueryString = PostgresConnectionStringParser.Parse(urlWithQuery, trustServerCertificate: false);

            // Assert
            fromConfigFlag.Should().Contain("SSL Mode=Require");
            fromConfigFlag.Should().Contain("Trust Server Certificate=true");

            fromQueryString.Should().Contain("Port=5433");
            fromQueryString.Should().Contain("SSL Mode=VerifyFull");
            fromQueryString.Should().Contain("Trust Server Certificate=true");
        }

        private static string DescribeOperations(IReadOnlyList<MigrationOperation> operations)
        {
            if (operations.Count == 0)
            {
                return "ninguna";
            }

            var sb = new StringBuilder();
            foreach (var op in operations.Take(25))
            {
                sb.AppendLine();
                switch (op)
                {
                    case AddColumnOperation addCol:
                        sb.Append($"AddColumn {addCol.Table}.{addCol.Name} ({addCol.ClrType.Name}, nullable={addCol.IsNullable}, default={addCol.DefaultValue})");
                        break;
                    case DropColumnOperation dropCol:
                        sb.Append($"DropColumn {dropCol.Table}.{dropCol.Name}");
                        break;
                    case AlterColumnOperation alterCol:
                        sb.Append($"AlterColumn {alterCol.Table}.{alterCol.Name} ({alterCol.ClrType.Name}, type={alterCol.ColumnType}, nullable={alterCol.IsNullable}, default={alterCol.DefaultValue})");
                        break;
                    case CreateTableOperation createTable:
                        sb.Append($"CreateTable {createTable.Name}");
                        break;
                    case DropTableOperation dropTable:
                        sb.Append($"DropTable {dropTable.Name}");
                        break;
                    case CreateIndexOperation createIndex:
                        sb.Append($"CreateIndex {createIndex.Table}.{createIndex.Name} on [{string.Join(",", createIndex.Columns)}]");
                        break;
                    case DropIndexOperation dropIndex:
                        sb.Append($"DropIndex {dropIndex.Table}.{dropIndex.Name}");
                        break;
                    case AddForeignKeyOperation addFk:
                        sb.Append($"AddFK {addFk.Table}.{addFk.Name} -> {addFk.PrincipalTable}");
                        break;
                    case DropForeignKeyOperation dropFk:
                        sb.Append($"DropFK {dropFk.Table}.{dropFk.Name}");
                        break;
                    default:
                        sb.Append(op.GetType().Name);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
