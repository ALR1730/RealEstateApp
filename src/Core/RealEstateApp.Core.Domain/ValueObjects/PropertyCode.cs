using System;
using System.Text.RegularExpressions;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Domain.ValueObjects
{
    public sealed record PropertyCode
    {
        public const int RequiredLength = 6;

        private static readonly Regex AlphanumericCodeRegex = new(
            "^[A-Z0-9]{6}$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(200));

        public string Value { get; }

        public PropertyCode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new DomainException($"El código de propiedad debe tener exactamente {RequiredLength} caracteres alfanuméricos.");
            }

            var normalized = value.ToUpperInvariant();
            if (!AlphanumericCodeRegex.IsMatch(normalized))
            {
                throw new DomainException($"El código de propiedad '{value}' debe tener exactamente {RequiredLength} caracteres alfanuméricos ASCII.");
            }

            Value = normalized;
        }

        public static implicit operator string(PropertyCode code) => code.Value;

        public override string ToString() => Value;
    }
}
