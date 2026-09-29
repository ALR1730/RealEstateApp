using System;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstateApp.Core.Application.Helpers
{
    /// <summary>
    /// Utilidades puras para la generación y normalización de códigos y prefijos de propiedades.
    /// </summary>
    public static class PropertyCodeGenerator
    {
        public static async Task<string> GenerateUniqueCodeAsync(string prefix, Func<string, Task<bool>> codeExistsCheck)
        {
            string code;
            do
            {
                code = $"{prefix}{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }
            while (await codeExistsCheck(code));

            return code;
        }

        public static string GetPropertyTypePrefix(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return "PROP";

            var trimmed = typeName.Trim();
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (words.Length > 1)
            {
                var initials = new string(words.Select(w => char.ToUpper(w[0])).ToArray());
                if (initials.Length >= 3)
                    return initials[..3];

                var firstWordChar = char.ToUpper(words[0][0]);
                var secondWordClean = new string(words[1].Where(char.IsLetterOrDigit).ToArray()).ToUpper();
                if (secondWordClean.Length >= 2)
                    return $"{firstWordChar}{secondWordClean[..2]}";

                return (firstWordChar + secondWordClean).PadRight(3, 'X');
            }

            var cleanWord = new string(trimmed.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            if (cleanWord.Length <= 3)
                return cleanWord.PadRight(3, 'X');

            // Coincidencias conocidas comunes para alta legibilidad
            if (cleanWord.StartsWith("APARTAM")) return "APT";
            if (cleanWord.StartsWith("VILL")) return "VIL";
            if (cleanWord.StartsWith("CASA")) return "CAS";
            if (cleanWord.StartsWith("PENTH")) return "PNT";
            if (cleanWord.StartsWith("TERRE")) return "TER";
            if (cleanWord.StartsWith("LOCAL")) return "LOC";
            if (cleanWord.StartsWith("EDIFI")) return "EDI";

            // Algoritmo dinámico para cualquier tipo de propiedad nuevo
            var firstChar = cleanWord[0];
            var consonants = cleanWord.Substring(1).Where(c => !"AEIOUáéíóúÁÉÍÓÚ".Contains(c)).ToArray();
            if (consonants.Length >= 2)
            {
                return $"{firstChar}{consonants[0]}{consonants[1]}";
            }

            return cleanWord[..3];
        }
    }
}
