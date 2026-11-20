using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RealEstateApp.Core.Domain.Exceptions;
using RealEstateApp.Core.Domain.Services;
using Xunit;

namespace RealEstateApp.UnitTests.Domain
{
    public class MortgageAmortizationCalculatorTests
    {
        #region Límites Inválidos - Lanzan ValidationException

        [Theory]
        [InlineData(0, 0, 10, 20)]
        [InlineData(-100000, 0, 10, 20)]
        [InlineData(1000000, -1, 10, 20)]
        [InlineData(1000000, 1000000, 10, 20)]
        [InlineData(1000000, 1200000, 10, 20)]
        [InlineData(1000000, 200000, -0.01, 20)]
        [InlineData(1000000, 200000, -5, 20)]
        [InlineData(1000000, 200000, 100.01, 20)]
        [InlineData(1000000, 200000, 150, 20)]
        [InlineData(1000000, 200000, 0, 0)]
        [InlineData(1000000, 200000, 0, -5)]
        [InlineData(1000000, 200000, 10, 0)]
        [InlineData(1000000, 200000, 10, -10)]
        [InlineData(1000000, 200000, 10, 41)]
        [InlineData(1000000, 200000, 10, 500000)]
        public void Calculate_DebeLanzarValidationException_CuandoParametrosEstanFueraDeRango(
            decimal propertyPrice,
            decimal downPayment,
            decimal annualRate,
            int termInYears)
        {
            // Arrange & Act
            Action act = () => MortgageAmortizationCalculator.Calculate(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            // Assert
            act.Should().Throw<ValidationException>();
        }

        [Theory]
        [InlineData(0, 0, 10, 20)]
        [InlineData(-1, 0, 10, 20)]
        [InlineData(1000000, -100, 10, 20)]
        [InlineData(1000000, 1000000, 10, 20)]
        [InlineData(1000000, 1500000, 10, 20)]
        [InlineData(1000000, 200000, -1, 20)]
        [InlineData(1000000, 200000, 101, 20)]
        [InlineData(1000000, 200000, 0, 0)]
        [InlineData(1000000, 200000, 0, -1)]
        [InlineData(1000000, 200000, 10, 0)]
        [InlineData(1000000, 200000, 10, -1)]
        [InlineData(1000000, 200000, 10, 41)]
        public void GenerateSchedule_DebeLanzarValidationException_CuandoParametrosEstanFueraDeRango(
            decimal propertyPrice,
            decimal downPayment,
            decimal annualRate,
            int termInYears)
        {
            // Arrange & Act
            Action act = () => MortgageAmortizationCalculator.GenerateSchedule(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            // Assert
            act.Should().Throw<ValidationException>();
        }

        #endregion

        #region Casos Válidos - Tasa 0%, Tasa 11.5% y Límites Válidos

        [Fact]
        public void Calculate_DebeCalcularSinDivisionPorCero_CuandoTasaAnualEsCero()
        {
            // Arrange
            const decimal propertyPrice = 1_200_000m;
            const decimal downPayment = 200_000m;
            const decimal annualRate = 0m;
            const int termInYears = 10;

            // Act
            MortgageCalculation result = MortgageAmortizationCalculator.Calculate(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            // Assert
            result.Should().NotBeNull();
            result.LoanAmount.Should().Be(1_000_000m);
            result.TotalMonths.Should().Be(120);
            result.MonthlyInstallment.Should().BeApproximately(8_333.33m, 0.05m);
            result.TotalInterest.Should().Be(0m);
            result.Schedule.Should().HaveCount(120);
            result.Schedule.Should().OnlyContain(item => item.Interest == 0m);
            result.Schedule[^1].RemainingBalance.Should().Be(0m);
        }

        [Fact]
        public void GenerateSchedule_DebeGenerarCuotasExactasSinDivisionPorCero_CuandoTasaAnualEsCero()
        {
            // Arrange
            const decimal propertyPrice = 600_000m;
            const decimal downPayment = 0m;
            const decimal annualRate = 0m;
            const int termInYears = 5;

            // Act
            IReadOnlyList<AmortizationInstallment> schedule =
                MortgageAmortizationCalculator.GenerateSchedule(propertyPrice, downPayment, annualRate, termInYears);

            // Assert
            schedule.Should().HaveCount(60);
            schedule[0].Period.Should().Be(1);
            schedule[0].Installment.Should().Be(10_000m);
            schedule[0].Interest.Should().Be(0m);
            schedule[0].Principal.Should().Be(10_000m);
            schedule[^1].Period.Should().Be(60);
            schedule[^1].RemainingBalance.Should().Be(0m);
        }

        [Fact]
        public void Calculate_DebeCalcularSistemaFrancesCorrectamente_CuandoTasaAnualEs11Punto5()
        {
            // Arrange
            const decimal propertyPrice = 5_000_000m;
            const decimal downPayment = 1_000_000m;
            const decimal annualRate = 11.5m;
            const int termInYears = 20;

            // Act
            MortgageCalculation result = MortgageAmortizationCalculator.Calculate(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            // Assert
            result.Should().NotBeNull();
            result.PropertyPrice.Should().Be(5_000_000m);
            result.DownPayment.Should().Be(1_000_000m);
            result.DownPaymentPercentage.Should().Be(20m);
            result.LoanAmount.Should().Be(4_000_000m);
            result.AnnualRate.Should().Be(11.5m);
            result.TermInYears.Should().Be(20);
            result.TotalMonths.Should().Be(240);
            result.MonthlyInstallment.Should().BeApproximately(42_657.20m, 1.00m);
            result.TotalInterest.Should().BeGreaterThan(0m);
            result.TotalCost.Should().BeGreaterThan(result.LoanAmount);
            result.Schedule.Should().HaveCount(240);
            result.Schedule[0].Period.Should().Be(1);
            result.Schedule[0].Interest.Should().BeGreaterThan(0m);
            result.Schedule[0].Principal.Should().BeGreaterThan(0m);
            result.Schedule[0].RemainingBalance.Should().BeLessThan(4_000_000m);
            result.Schedule[^1].Period.Should().Be(240);
            result.Schedule[^1].RemainingBalance.Should().BeApproximately(0m, 0.05m);
        }

        [Fact]
        public void GenerateSchedule_DebeSerConsistenteConCalculateYReducirSaldoHastaCero_CuandoTasaEs11Punto5()
        {
            // Arrange
            const decimal propertyPrice = 5_000_000m;
            const decimal downPayment = 1_000_000m;
            const decimal annualRate = 11.5m;
            const int termInYears = 20;

            // Act
            MortgageCalculation calculation = MortgageAmortizationCalculator.Calculate(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);
            IReadOnlyList<AmortizationInstallment> schedule = MortgageAmortizationCalculator.GenerateSchedule(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            // Assert
            schedule.Should().HaveCount(240);
            schedule.Should().BeEquivalentTo(calculation.Schedule, options => options.WithStrictOrdering());
            schedule.Select(s => s.Period).Should().BeEquivalentTo(Enumerable.Range(1, 240));
            schedule[^1].RemainingBalance.Should().BeApproximately(0m, 0.05m);
        }

        [Theory]
        [InlineData(1_000_000, 0, 100, 1)]
        [InlineData(1_000_000, 0, 8.5, 40)]
        public void Calculate_DebePermitirLimitesValidosDePlazoYTasa(
            decimal propertyPrice,
            decimal downPayment,
            decimal annualRate,
            int termInYears)
        {
            // Arrange & Act
            MortgageCalculation result = MortgageAmortizationCalculator.Calculate(
                propertyPrice,
                downPayment,
                annualRate,
                termInYears);

            // Assert
            result.TotalMonths.Should().Be(termInYears * 12);
            result.Schedule.Should().HaveCount(termInYears * 12);
            result.Schedule[^1].RemainingBalance.Should().BeApproximately(0m, 0.05m);
        }

        #endregion
    }
}
