using System;
using FluentAssertions;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Core.Domain.ValueObjects;
using Xunit;

namespace RealEstateApp.UnitTests.Domain
{
    public class ValueObjectsTests
    {
        #region Money

        [Fact]
        public void Money_DebeCrearInstanciaConMonedaPorDefectoDOP_CuandoSoloSePasaMonto()
        {
            // Arrange & Act
            var money = new Money(1500.50m);

            // Assert
            money.Amount.Should().Be(1500.50m);
            money.Currency.Should().Be(CurrencyConstants.DOP);
        }

        [Theory]
        [InlineData("dop", "DOP")]
        [InlineData("usd", "USD")]
        [InlineData("Usd", "USD")]
        [InlineData("dOp", "DOP")]
        public void Money_DebeNormalizarMonedaAMayusculas_CuandoMonedaEsValida(
            string inputCurrency,
            string expectedCurrency)
        {
            // Arrange & Act
            var money = new Money(100m, inputCurrency);

            // Assert
            money.Currency.Should().Be(expectedCurrency);
        }

        [Theory]
        [InlineData(1234.567, 1234.57)]
        [InlineData(1234.564, 1234.56)]
        [InlineData(0.0, 0.00)]
        public void Money_DebeRedondearMontoADosDecimales_CuandoTieneMasDecimales(
            decimal rawAmount,
            decimal expectedAmount)
        {
            // Arrange & Act
            var money = new Money(rawAmount, CurrencyConstants.DOP);

            // Assert
            money.Amount.Should().Be(expectedAmount);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-1000)]
        public void Money_DebeLanzarDomainException_CuandoMontoEsNegativo(decimal negativeAmount)
        {
            // Arrange & Act
            Action act = () => _ = new Money(negativeAmount, CurrencyConstants.DOP);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("EUR")]
        [InlineData("BTC")]
        [InlineData("DOP'; DROP TABLE Properties;--")]
        public void Money_DebeLanzarDomainException_CuandoMonedaEsNulaVaciaONoSoportada(string? invalidCurrency)
        {
            // Arrange & Act
            Action act = () => _ = new Money(100m, invalidCurrency!);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void MoneyFactories_DebeCrearInstanciasCorrectas_ParaZeroDopYUsd()
        {
            // Arrange & Act
            var zeroDefault = Money.Zero();
            var zeroUsd = Money.Zero(CurrencyConstants.USD);
            var dop = Money.Dop(2500.456m);
            var usd = Money.Usd(125.789m);

            // Assert
            zeroDefault.Amount.Should().Be(0m);
            zeroDefault.Currency.Should().Be(CurrencyConstants.DOP);

            zeroUsd.Amount.Should().Be(0m);
            zeroUsd.Currency.Should().Be(CurrencyConstants.USD);

            dop.Amount.Should().Be(2500.46m);
            dop.Currency.Should().Be(CurrencyConstants.DOP);

            usd.Amount.Should().Be(125.79m);
            usd.Currency.Should().Be(CurrencyConstants.USD);
        }

        [Fact]
        public void ConvertToDop_DebeConvertirUsandoTasaPorDefecto_CuandoMonedaEsUSD()
        {
            // Arrange
            var usd = Money.Usd(100m);
            var expectedAmount = Math.Round(100m * CurrencyConstants.DefaultUsdToDopRate, 2);

            // Act
            var converted = usd.ConvertToDop();

            // Assert
            converted.Currency.Should().Be(CurrencyConstants.DOP);
            converted.Amount.Should().Be(expectedAmount);
        }

        [Fact]
        public void ConvertToDop_DebeConvertirUsandoTasaPersonalizada_CuandoMonedaEsUSD()
        {
            // Arrange
            var usd = Money.Usd(200m);

            // Act
            var converted = usd.ConvertToDop(59.25m);

            // Assert
            converted.Currency.Should().Be(CurrencyConstants.DOP);
            converted.Amount.Should().Be(11_850.00m);
        }

        [Fact]
        public void ConvertToDop_DebeMantenerMonto_CuandoMonedaYaEsDOP()
        {
            // Arrange
            var dop = Money.Dop(5_000m);

            // Act
            var converted = dop.ConvertToDop(60.00m);

            // Assert
            converted.Should().Be(Money.Dop(5_000m));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-60.5)]
        public void ConvertToDop_DebeLanzarDomainException_CuandoTasaEsCeroONegativa(decimal invalidRate)
        {
            // Arrange
            var usd = Money.Usd(100m);

            // Act
            Action act = () => usd.ConvertToDop(invalidRate);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Add_DebeSumarMontos_CuandoMonedasSonIguales()
        {
            // Arrange
            var first = Money.Dop(100.25m);
            var second = Money.Dop(49.75m);

            // Act
            var result = first.Add(second);

            // Assert
            result.Should().Be(Money.Dop(150.00m));
        }

        [Fact]
        public void Add_DebeLanzarDomainException_CuandoMonedasSonDistintas()
        {
            // Arrange
            var dop = Money.Dop(100m);
            var usd = Money.Usd(50m);

            // Act
            Action act = () => dop.Add(usd);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Subtract_DebeRestarMontos_CuandoMonedasSonIgualesYResultadoEsCeroOPositivo()
        {
            // Arrange
            var total = Money.Usd(200m);
            var deduction = Money.Usd(50m);

            // Act
            var remaining = total.Subtract(deduction);
            var exactZero = deduction.Subtract(Money.Usd(50m));

            // Assert
            remaining.Should().Be(Money.Usd(150m));
            exactZero.Should().Be(Money.Zero(CurrencyConstants.USD));
        }

        [Fact]
        public void Subtract_DebeLanzarDomainException_CuandoMonedasSonDistintas()
        {
            // Arrange
            var dop = Money.Dop(500m);
            var usd = Money.Usd(10m);

            // Act
            Action act = () => dop.Subtract(usd);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Subtract_DebeLanzarDomainException_CuandoResultadoEsNegativo()
        {
            // Arrange
            var small = Money.Dop(50m);
            var large = Money.Dop(50.01m);

            // Act
            Action act = () => small.Subtract(large);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Money_DebeCompararPorValorEstructuralmente_AlSerRecord()
        {
            // Arrange
            var a = new Money(100.00m, "dop");
            var b = Money.Dop(100m);
            var c = Money.Usd(100m);

            // Act & Assert
            a.Should().Be(b);
            (a == b).Should().BeTrue();
            a.Should().NotBe(c);
        }

        #endregion

        #region PropertyCode

        [Fact]
        public void PropertyCode_DebeTenerLongitudRequeridaDeSeis()
        {
            // Assert
            PropertyCode.RequiredLength.Should().Be(6);
        }

        [Theory]
        [InlineData("ab12cd", "AB12CD")]
        [InlineData("X9y8Z7", "X9Y8Z7")]
        [InlineData("000123", "000123")]
        public void PropertyCode_DebeAceptarSeisAlfanumericosYNormalizarAMayusculas(
            string rawCode,
            string expectedNormalized)
        {
            // Arrange & Act
            var code = new PropertyCode(rawCode);
            string implicitValue = code;

            // Assert
            code.Value.Should().Be(expectedNormalized);
            code.ToString().Should().Be(expectedNormalized);
            implicitValue.Should().Be(expectedNormalized);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("      ")]
        [InlineData("ABC12")]
        [InlineData("ABC1234")]
        [InlineData("ABC-12")]
        [InlineData("ABC 12")]
        [InlineData("ABC_12")]
        [InlineData("ABC12!")]
        [InlineData("ABÑ123")]
        public void PropertyCode_DebeLanzarDomainException_CuandoFormatoEsInvalido(string? invalidCode)
        {
            // Arrange & Act
            Action act = () => _ = new PropertyCode(invalidCode!);

            // Assert
            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void PropertyCode_DebeCompararPorValorEstructuralmente_AlSerRecord()
        {
            // Arrange
            var code1 = new PropertyCode("vil101");
            var code2 = new PropertyCode("VIL101");
            var code3 = new PropertyCode("APT202");

            // Act & Assert
            code1.Should().Be(code2);
            (code1 == code2).Should().BeTrue();
            code1.Should().NotBe(code3);
        }

        #endregion
    }
}
