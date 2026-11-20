using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RealEstateApp.Core.Application.Services.AiSearch
{
    public static class AiSearchTextHelper
    {
        public static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

        public static string NormalizeText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var decomposed = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(c);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        public static bool SafeIsMatch(string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(input, pattern, options | RegexOptions.CultureInvariant, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static Match SafeMatch(string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Match.Empty;
            }

            try
            {
                return Regex.Match(input, pattern, options | RegexOptions.CultureInvariant, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return Match.Empty;
            }
        }

        public static bool MatchesWord(string normalizedQuery, string normalizedTerm)
        {
            if (string.IsNullOrWhiteSpace(normalizedTerm))
            {
                return false;
            }

            return SafeIsMatch(normalizedQuery, $@"\b{Regex.Escape(normalizedTerm)}\b");
        }

        public static bool MatchesCatalogSynonyms(
            string normalizedQuery,
            string normalizedEntityName,
            IReadOnlyDictionary<string, string[]> synonyms)
        {
            foreach (var entry in synonyms)
            {
                var keyNormalized = NormalizeText(entry.Key);
                if (!normalizedEntityName.Contains(keyNormalized, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (entry.Value.Any(term => MatchesWord(normalizedQuery, term)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
