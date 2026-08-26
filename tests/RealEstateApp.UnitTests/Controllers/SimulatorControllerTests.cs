using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Presentation.WebApi.Controllers.v1;
using Xunit;

namespace RealEstateApp.UnitTests.Controllers
{
    public class SimulatorControllerTests
    {
        private readonly Mock<IFinancingService> _financingServiceMock;
        private readonly SimulatorController _controller;

        public SimulatorControllerTests()
        {
            _financingServiceMock = new Mock<IFinancingService>();
            _controller = new SimulatorController(_financingServiceMock.Object);
        }

        [Fact]
        public void Calculate_ValidParameters_ReturnsOkWithSimulationResult()
        {
            // Arrange
            var request = new SimulatorController.MortgageRequest
            {
                PropertyPrice = 5000000m,
                DownPayment = 1000000m,
                AnnualRate = 11.5m,
                TermInYears = 20
            };

            var expectedResult = new MortgageSimulationResult
            {
                PropertyPrice = 5000000m,
                DownPayment = 1000000m,
                LoanAmount = 4000000m,
                AnnualRate = 11.5m,
                TermInYears = 20,
                TotalMonths = 240,
                MonthlyInstallment = 42633.72m,
                TotalInterest = 6232092.80m,
                TotalCost = 10232092.80m,
                Schedule = new List<AmortizationScheduleItem>()
            };

            _financingServiceMock.Setup(s => s.CalculateMortgage(request.PropertyPrice, request.DownPayment, request.AnnualRate, request.TermInYears))
                .Returns(expectedResult);

            // Act
            var result = _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void Calculate_InvalidPrice_ReturnsBadRequest()
        {
            // Arrange
            var request = new SimulatorController.MortgageRequest
            {
                PropertyPrice = 0m,
                DownPayment = 0m
            };

            // Act
            var result = _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
