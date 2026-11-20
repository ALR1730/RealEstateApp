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

            var normalizedInput = connectionString.Trim();

            if (!normalizedInput.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
                !normalizedInput.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                return AppendTrustFlagIfRequested(normalizedInput, trustServerCertificate);
            }

            if (!Uri.TryCreate(normalizedInput, UriKind.Absolute, out var uri))
            {
                return connectionString;
            }

            return BuildConnectionStringFromUri(uri, trustServerCertificate);
        }

        private static string BuildConnectionStringFromUri(Uri uri, bool trustServerCertificate)
        {
            var (user, pass) = ExtractCredentials(uri.UserInfo);
            var host = FormatConnectionStringValue(uri.Host);
            var port = uri.Port > 0 ? uri.Port : 5432;
            var db = FormatConnectionStringValue(uri.AbsolutePath.TrimStart('/'));

            var queryParams = ParseQueryParameters(uri.Query);
            var sslMode = queryParams.TryGetValue("sslmode", out var parsedSslMode)
                ? NormalizeSslMode(parsedSslMode)
                : "Require";

            var hasChannelBinding = queryParams.TryGetValue("channel_binding", out var parsedChannelBinding)
                && !string.IsNullOrWhiteSpace(parsedChannelBinding);

            var trustFromQuery = queryParams.TryGetValue("trustservercertificate", out var parsedTrust)
                && string.Equals(parsedTrust, "true", StringComparison.OrdinalIgnoreCase);

            var sb = new StringBuilder();
            sb.Append($"Host={host};Port={port};Database={db};Username={FormatConnectionStringValue(user)};Password={FormatConnectionStringValue(pass)};SSL Mode={sslMode};");

            if (hasChannelBinding)
            {
                sb.Append($"Channel Binding={NormalizeChannelBinding(parsedChannelBinding!)};");
            }

            if (trustServerCertificate || trustFromQuery)
            {
                sb.Append("Trust Server Certificate=true;");
            }

            return sb.ToString();
        }

        private static (string User, string Pass) ExtractCredentials(string userInfo)
        {
            if (string.IsNullOrEmpty(userInfo))
            {
                return (string.Empty, string.Empty);
            }

            var parts = userInfo.Split(':', 2);
            var user = Uri.UnescapeDataString(parts[0]);
            var pass = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            return (user, pass);
        }

        private static string NormalizeSslMode(string? rawSslMode)
        {
            if (string.IsNullOrWhiteSpace(rawSslMode))
            {
                return "Require";
            }

            var trimmed = rawSslMode.Trim();
            return trimmed.ToLowerInvariant() switch
            {
                "disable" => "Disable",
                "allow" => "Allow",
                "prefer" => "Prefer",
                "require" => "Require",
                "verify-ca" or "verifyca" => "VerifyCA",
                "verify-full" or "verifyfull" => "VerifyFull",
                _ => trimmed
            };
        }

        private static string NormalizeChannelBinding(string rawChannelBinding)
        {
            var trimmed = rawChannelBinding.Trim();
            return trimmed.ToLowerInvariant() switch
            {
                "disable" => "Disable",
                "prefer" => "Prefer",
                "require" => "Require",
                _ => trimmed
            };
        }

        private static string FormatConnectionStringValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var hasEdgeWhitespace = char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]);
            var needsQuoting = hasEdgeWhitespace
                || value.Contains(';', StringComparison.Ordinal)
                || value.Contains('\'', StringComparison.Ordinal)
                || value.Contains('"', StringComparison.Ordinal);

            return needsQuoting
                ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
                : value;
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
