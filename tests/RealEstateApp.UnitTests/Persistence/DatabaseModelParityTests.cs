using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence;
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

        [Fact]
        public void DatabaseConnectionResolver_DebePriorizarDatabaseUrlSobreDefaultConnection_CuandoAmbosEstanPresentes()
        {
            // Arrange
            const string neonUrl = "postgresql://neondb_owner:npg_Secret@ep-test-pooler.us-east-2.aws.neon.tech/neondb?sslmode=require&channel_binding=require";
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDatabase"] = "false",
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=RealEstateAppDb;Trusted_Connection=True;MultipleActiveResultSets=true",
                    ["DATABASE_URL"] = neonUrl
                })
                .Build();

            // Act
            var settings = DatabaseConnectionResolver.Resolve(configuration);

            // Assert
            settings.Provider.Should().Be(DatabaseProviderType.PostgreSql);
            settings.ConnectionString.Should().NotBeNullOrWhiteSpace();

            var builder = new NpgsqlConnectionStringBuilder(settings.ConnectionString);
            builder.Host.Should().Be("ep-test-pooler.us-east-2.aws.neon.tech");
            builder.Port.Should().Be(5432);
            builder.Database.Should().Be("neondb");
            builder.Username.Should().Be("neondb_owner");
            builder.Password.Should().Be("npg_Secret");
            builder.SslMode.Should().Be(SslMode.Require);
            builder.ChannelBinding.Should().Be(Npgsql.ChannelBinding.Require);
        }

        [Fact]
        public void DatabaseConnectionResolver_DebeUsarSqlServerDefaultConnection_CuandoDatabaseUrlEstaVacio()
        {
            // Arrange
            const string localConnection = "Server=(localdb)\\MSSQLLocalDB;Database=RealEstateAppDb;Trusted_Connection=True;";
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDatabase"] = "false",
                    ["DATABASE_URL"] = "   ",
                    ["ConnectionStrings:DefaultConnection"] = localConnection
                })
                .Build();

            // Act
            var settings = DatabaseConnectionResolver.Resolve(configuration);

            // Assert
            settings.Provider.Should().Be(DatabaseProviderType.SqlServer);
            settings.ConnectionString.Should().Be(localConnection);
        }

        [Fact]
        public void DatabaseConnectionResolver_DebeUsarInMemory_CuandoUseInMemoryDatabaseEsTrueInclusoConDatabaseUrl()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDatabase"] = "true",
                    ["DATABASE_URL"] = "postgresql://neondb_owner:npg_Secret@ep-test-pooler.us-east-2.aws.neon.tech/neondb?sslmode=require"
                })
                .Build();

            // Act
            var settings = DatabaseConnectionResolver.Resolve(configuration);

            // Assert
            settings.Provider.Should().Be(DatabaseProviderType.InMemory);
            settings.ConnectionString.Should().BeNull();
        }

        [Fact]
        public void PostgresConnectionStringParser_DebeNormalizarParametrosDeNeon_CuandoUsaVerifyFullYChannelBinding()
        {
            // Arrange
            const string neonUrl = "postgresql://neondb_owner:npg_Pass123@ep-cool-pooler.us-east-2.aws.neon.tech/neondb?sslmode=verify-full&channel_binding=require";

            // Act
            var parsed = PostgresConnectionStringParser.Parse(neonUrl);

            // Assert
            var builder = new NpgsqlConnectionStringBuilder(parsed);
            builder.Host.Should().Be("ep-cool-pooler.us-east-2.aws.neon.tech");
            builder.Port.Should().Be(5432);
            builder.Database.Should().Be("neondb");
            builder.Username.Should().Be("neondb_owner");
            builder.Password.Should().Be("npg_Pass123");
            builder.SslMode.Should().Be(SslMode.VerifyFull);
            builder.ChannelBinding.Should().Be(Npgsql.ChannelBinding.Require);
            parsed.Should().NotContainEquivalentOf("Trust Server Certificate");
        }

        [Fact]
        public void ServiceRegistration_DebeRegistrarProveedorNpgsql_CuandoDatabaseUrlCoexisteConDefaultConnection()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDatabase"] = "false",
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=RealEstateAppDb;Trusted_Connection=True;MultipleActiveResultSets=true",
                    ["DATABASE_URL"] = "postgresql://neondb_owner:npg_Secret@ep-test-pooler.us-east-2.aws.neon.tech/neondb?sslmode=verify-full&channel_binding=require"
                })
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPersistenceInfrastructure(configuration);

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act
            var isNpgsql = dbContext.Database.IsNpgsql();
            var isSqlServer = dbContext.Database.IsSqlServer();

            // Assert
            isNpgsql.Should().BeTrue();
            isSqlServer.Should().BeFalse();
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
