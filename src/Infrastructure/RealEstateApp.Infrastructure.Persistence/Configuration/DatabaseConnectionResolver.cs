using System;
using Microsoft.Extensions.Configuration;

namespace RealEstateApp.Infrastructure.Persistence.Configuration
{
    /// <summary>
    /// Proveedores de base de datos soportados por la infraestructura de persistencia.
    /// </summary>
    public enum DatabaseProviderType
    {
        InMemory,
        PostgreSql,
        SqlServer
    }

    /// <summary>
    /// Resultado resuelto de proveedor y cadena de conexión lista para EF Core.
    /// </summary>
    public sealed record DatabaseConnectionSettings(
        DatabaseProviderType Provider,
        string? ConnectionString);

    /// <summary>
    /// Resuelve el proveedor de base de datos y la cadena de conexión priorizando
    /// variables de entorno cloud (<c>DATABASE_URL</c>) sobre el fallback local.
    /// </summary>
    public static class DatabaseConnectionResolver
    {
        public static DatabaseConnectionSettings Resolve(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                return new DatabaseConnectionSettings(DatabaseProviderType.InMemory, null);
            }

            var rawConnectionString = FirstNonEmpty(
                configuration["DATABASE_URL"],
                configuration["DATABASE_URL_INTERNAL"],
                configuration.GetConnectionString("DefaultConnection"));

            if (IsPostgreSqlConfigured(configuration, rawConnectionString))
            {
                var trustServerCertificate = configuration.GetValue<bool>("Database:TrustServerCertificate")
                    || string.Equals(configuration["DB_TRUST_SERVER_CERTIFICATE"], "true", StringComparison.OrdinalIgnoreCase);

                var parsedConnection = string.IsNullOrWhiteSpace(rawConnectionString)
                    ? rawConnectionString
                    : PostgresConnectionStringParser.Parse(rawConnectionString, trustServerCertificate);

                return new DatabaseConnectionSettings(DatabaseProviderType.PostgreSql, parsedConnection);
            }

            return new DatabaseConnectionSettings(DatabaseProviderType.SqlServer, rawConnectionString);
        }

        private static bool IsPostgreSqlConfigured(IConfiguration configuration, string? connectionString)
        {
            if (configuration.GetValue<bool>("UsePostgreSQL"))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            return connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
                || (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase)
                    && connectionString.Contains("Port=5432", StringComparison.OrdinalIgnoreCase));
        }

        private static string? FirstNonEmpty(params string?[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim();
                }
            }

            return null;
        }
    }
}
