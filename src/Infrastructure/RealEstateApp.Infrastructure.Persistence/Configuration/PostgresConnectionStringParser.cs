using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateApp.Infrastructure.Persistence.Configuration
{
    public static class PostgresConnectionStringParser
    {
        public static string Parse(string connectionString, bool trustServerCertificate = false)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
                !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                return AppendTrustFlagIfRequested(connectionString, trustServerCertificate);
            }

            if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            {
                return connectionString;
            }

            var userInfo = uri.UserInfo.Split(':', 2);
            var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
            var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var db = uri.AbsolutePath.TrimStart('/');

            var queryParams = ParseQueryParameters(uri.Query);
            var sslMode = queryParams.TryGetValue("sslmode", out var parsedSslMode) && !string.IsNullOrWhiteSpace(parsedSslMode)
                ? parsedSslMode
                : "Require";

            var trustFromQuery = queryParams.TryGetValue("trustservercertificate", out var parsedTrust)
                && string.Equals(parsedTrust, "true", StringComparison.OrdinalIgnoreCase);

            var sb = new StringBuilder();
            sb.Append($"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode={sslMode};");

            if (trustServerCertificate || trustFromQuery)
            {
                sb.Append("Trust Server Certificate=true;");
            }

            return sb.ToString();
        }

        private static string AppendTrustFlagIfRequested(string connectionString, bool trustServerCertificate)
        {
            if (!trustServerCertificate ||
                connectionString.Contains("Trust Server Certificate", StringComparison.OrdinalIgnoreCase))
            {
                return connectionString;
            }

            var trimmed = connectionString.TrimEnd();
            var separator = trimmed.EndsWith(";", StringComparison.Ordinal) ? string.Empty : ";";
            return $"{trimmed}{separator}Trust Server Certificate=true;";
        }

        private static Dictionary<string, string> ParseQueryParameters(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }

            foreach (var segment in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = segment.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = Uri.UnescapeDataString(parts[0]).Trim();
                    var val = Uri.UnescapeDataString(parts[1]).Trim();
                    result[key] = val;
                }
            }

            return result;
        }
    }
}
