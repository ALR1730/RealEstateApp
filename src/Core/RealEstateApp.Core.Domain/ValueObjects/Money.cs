using System;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Domain.ValueObjects
{
    public sealed record Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency = CurrencyConstants.DOP)
        {
            if (amount < 0)
            {
                throw new DomainException("El monto monetario no puede ser negativo.");
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new DomainException("La moneda es obligatoria.");
            }

            var normalizedCurrency = currency.Trim().ToUpperInvariant();
            if (normalizedCurrency != CurrencyConstants.DOP && normalizedCurrency != CurrencyConstants.USD)
            {
                throw new DomainException($"La moneda '{currency}' no es soportada. Use DOP o USD.");
            }

            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
            Currency = normalizedCurrency;
        }

        public static Money Zero(string currency = CurrencyConstants.DOP) => new(0m, currency);

        public static Money Dop(decimal amount) => new(amount, CurrencyConstants.DOP);

        public static Money Usd(decimal amount) => new(amount, CurrencyConstants.USD);

        public Money ConvertToDop(decimal usdToDopRate = CurrencyConstants.DefaultUsdToDopRate)
        {
            if (usdToDopRate <= 0)
            {
                throw new DomainException("La tasa de cambio USD a DOP debe ser mayor a cero.");
            }

            return Currency == CurrencyConstants.DOP
                ? this
                : Dop(Amount * usdToDopRate);
        }

        public Money Add(Money other)
        {
            if (other is null)
            {
                throw new DomainException("El monto a sumar no puede ser nulo.");
            }

            if (Currency != other.Currency)
            {
                throw new DomainException($"No se pueden sumar monedas distintas ({Currency} y {other.Currency}).");
            }

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (other is null)
            {
                throw new DomainException("El monto a restar no puede ser nulo.");
            }

            if (Currency != other.Currency)
            {
                throw new DomainException($"No se pueden restar monedas distintas ({Currency} y {other.Currency}).");
            }

            if (Amount < other.Amount)
            {
                throw new DomainException("El resultado de la resta monetaria no puede ser negativo.");
            }

            return new Money(Amount - other.Amount, Currency);
        }
    }
}
