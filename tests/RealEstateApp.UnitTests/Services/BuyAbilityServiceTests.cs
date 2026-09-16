using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.Services;
using RealEstateApp.Core.Application.ViewModels.BuyAbility;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;
using Xunit;

namespace RealEstateApp.UnitTests.Services
{
    public class BuyAbilityServiceTests
    {
        private readonly Mock<IBuyAbilityEvaluationRepository> _repositoryMock;
        private readonly Mock<ICurrencyService> _currencyServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly BuyAbilityService _sut;

        public BuyAbilityServiceTests()
        {
            _repositoryMock = new Mock<IBuyAbilityEvaluationRepository>();
            _currencyServiceMock = new Mock<ICurrencyService>();
            _mapperMock = new Mock<IMapper>();

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<BuyAbilityEvaluation>());
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<BuyAbilityEvaluation>()))
                .ReturnsAsync((BuyAbilityEvaluation e) =>
                {
                    e.Id = 1;
                    return e;
                });

            _sut = new BuyAbilityService(
                _repositoryMock.Object,
                _currencyServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task EvaluateAsync_ShouldThrowValidationException_WhenGrossIncomeIsNegative()
        {
            // Arrange
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = -50000m,
                MonthlyNetIncome = 40000m,
                MonthlyDebtPayments = 5000m,
                AvailableDownPayment = 100000m
            };

            // Act
            Func<Task> act = async () => await _sut.EvaluateAsync("client-1", request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("El ingreso mensual bruto no puede ser negativo.");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldThrowValidationException_WhenGrossIncomeIsZero()
        {
            // Arrange
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 0m,
                MonthlyNetIncome = 40000m,
                MonthlyDebtPayments = 5000m,
                AvailableDownPayment = 100000m
            };

            // Act
            Func<Task> act = async () => await _sut.EvaluateAsync("client-1", request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("El ingreso mensual bruto debe ser mayor a cero.");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldThrowValidationException_WhenNetIncomeIsGreaterThanGrossIncome()
        {
            // Arrange
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 50000m,
                MonthlyNetIncome = 80000m,
                MonthlyDebtPayments = 5000m,
                AvailableDownPayment = 100000m
            };

            // Act
            Func<Task> act = async () => await _sut.EvaluateAsync("client-1", request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("El ingreso mensual neto no puede ser mayor que el ingreso mensual bruto.");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldThrowValidationException_WhenDebtPaymentsAreNegative()
        {
            // Arrange
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 50000m,
                MonthlyNetIncome = 40000m,
                MonthlyDebtPayments = -1000m,
                AvailableDownPayment = 100000m
            };

            // Act
            Func<Task> act = async () => await _sut.EvaluateAsync("client-1", request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("El total de deudas mensuales no puede ser negativo.");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldThrowValidationException_WhenDownPaymentIsNegative()
        {
            // Arrange
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 50000m,
                MonthlyNetIncome = 40000m,
                MonthlyDebtPayments = 5000m,
                AvailableDownPayment = -50000m
            };

            // Act
            Func<Task> act = async () => await _sut.EvaluateAsync("client-1", request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("El pago inicial disponible no puede ser negativo.");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldThrowValidationException_WhenAmountExceedsMaximumLimit()
        {
            // Arrange
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 2_000_000_000m,
                MonthlyNetIncome = 1_500_000_000m,
                MonthlyDebtPayments = 1000m,
                AvailableDownPayment = 50000m
            };

            // Act
            Func<Task> act = async () => await _sut.EvaluateAsync("client-1", request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Los montos ingresados no pueden exceder RD$ 1,000,000,000.");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldCalculateCorrectDtiAndApprove_WhenDtiIsBelow43AndDownPaymentIsPositive()
        {
            // Arrange (Net: 50,000, Debts: 4,500 -> DTI = 9.0%)
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 65000m,
                MonthlyNetIncome = 50000m,
                MonthlyDebtPayments = 4500m,
                AvailableDownPayment = 300000m,
                Currency = "DOP"
            };

            // Act
            var result = await _sut.EvaluateAsync("client-1", request);

            // Assert
            result.Should().NotBeNull();
            result.DebtToIncomeRatio.Should().Be(9.00m);
            result.CreditScoreRating.Should().Be("Excelente");
            result.EstimatedAnnualRate.Should().Be(9.5m);
            result.RecommendedTermYears.Should().Be(25);
            result.EvaluationResult.Should().Be("Aprobado");
            result.Observations.Should().Contain("RD$");
            result.Observations.Should().NotContain("RD$ ");
            result.Observations.Should().Contain("Felicidades");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldPreApprove_WhenDtiIsBetween43And50()
        {
            // Arrange (Net: 50,000, Debts: 23,000 -> DTI = 46.0%)
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 60000m,
                MonthlyNetIncome = 50000m,
                MonthlyDebtPayments = 23000m,
                AvailableDownPayment = 200000m
            };

            // Act
            var result = await _sut.EvaluateAsync("client-1", request);

            // Assert
            result.DebtToIncomeRatio.Should().Be(46.00m);
            result.EvaluationResult.Should().Be("Pre-Aprobado");
            result.Observations.Should().Contain("pre-aprobación");
        }

        [Fact]
        public async Task EvaluateAsync_ShouldNotApprove_WhenDtiIs50OrHigher()
        {
            // Arrange (Net: 40,000, Debts: 24,000 -> DTI = 60.0%)
            var request = new BuyAbilityRequestDto
            {
                MonthlyGrossIncome = 50000m,
                MonthlyNetIncome = 40000m,
                MonthlyDebtPayments = 24000m,
                AvailableDownPayment = 100000m
            };

            // Act
            var result = await _sut.EvaluateAsync("client-1", request);

            // Assert
            result.DebtToIncomeRatio.Should().Be(60.00m);
            result.EvaluationResult.Should().Be("No Aprobado");
            result.Observations.Should().Contain("Actualmente no cumple con los parámetros mínimos");
        }

        [Fact]
        public async Task GetLastEvaluationAsync_ShouldReturnNull_WhenNoEvaluationExists()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetLatestByClientIdAsync("client-none"))
                .ReturnsAsync((BuyAbilityEvaluation?)null);

            // Act
            var result = await _sut.GetLastEvaluationAsync("client-none");

            // Assert
            result.Should().BeNull();
        }
    }
}
