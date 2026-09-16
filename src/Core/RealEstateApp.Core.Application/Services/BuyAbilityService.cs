using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.BuyAbility;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    public class BuyAbilityService : IBuyAbilityService
    {
        private readonly IBuyAbilityEvaluationRepository _evaluationRepository;
        private readonly ICurrencyService _currencyService;
        private readonly IMapper _mapper;

        public BuyAbilityService(
            IBuyAbilityEvaluationRepository evaluationRepository,
            ICurrencyService currencyService,
            IMapper mapper)
        {
            _evaluationRepository = evaluationRepository;
            _currencyService = currencyService;
            _mapper = mapper;
        }

        public async Task<BuyAbilityResultDto> EvaluateAsync(string clientId, BuyAbilityRequestDto request)
        {
            ValidateInputs(request);

            var dti = CalculateDebtToIncomeRatio(request.MonthlyDebtPayments, request.MonthlyNetIncome);
            var creditScore = DetermineCreditScoreRating(dti);
            var annualRate = DetermineEstimatedAnnualRate(creditScore);
            var termYears = DetermineRecommendedTermYears(creditScore);
            var maxMonthlyPayment = CalculateMaxMonthlyPayment(request.MonthlyNetIncome);
            var maxMortgageAmount = CalculateMaxMortgageAmount(maxMonthlyPayment, annualRate, termYears);
            var maxPropertyPrice = maxMortgageAmount + request.AvailableDownPayment;
            var evaluationResult = DetermineEvaluationResult(dti, request.AvailableDownPayment);
            var observations = GenerateObservations(dti, creditScore, evaluationResult, maxPropertyPrice, annualRate, termYears);
            var currency = request.Currency ?? "DOP";

            await DeactivatePreviousEvaluationsAsync(clientId);

            var evaluation = new BuyAbilityEvaluation
            {
                ClientId = clientId,
                MonthlyGrossIncome = request.MonthlyGrossIncome,
                MonthlyNetIncome = request.MonthlyNetIncome,
                MonthlyDebtPayments = request.MonthlyDebtPayments,
                AvailableDownPayment = request.AvailableDownPayment,
                MaxMortgageAmount = Math.Round(maxMortgageAmount, 2),
                MaxPropertyPrice = Math.Round(maxPropertyPrice, 2),
                MaxMonthlyPayment = Math.Round(maxMonthlyPayment, 2),
                DebtToIncomeRatio = Math.Round(dti, 2),
                CreditScoreRating = creditScore,
                EstimatedAnnualRate = annualRate,
                RecommendedTermYears = termYears,
                Currency = currency,
                EvaluationResult = evaluationResult,
                EvaluationDate = DateTime.UtcNow,
                Observations = observations,
                IsActive = true
            };

            var saved = await _evaluationRepository.AddAsync(evaluation);

            return new BuyAbilityResultDto
            {
                EvaluationId = saved.Id,
                ClientId = clientId,
                MonthlyGrossIncome = request.MonthlyGrossIncome,
                MonthlyNetIncome = request.MonthlyNetIncome,
                MonthlyDebtPayments = request.MonthlyDebtPayments,
                AvailableDownPayment = request.AvailableDownPayment,
                MaxMonthlyPayment = Math.Round(maxMonthlyPayment, 2),
                MaxMortgageAmount = Math.Round(maxMortgageAmount, 2),
                MaxPropertyPrice = Math.Round(maxPropertyPrice, 2),
                DebtToIncomeRatio = Math.Round(dti, 2),
                CreditScoreRating = creditScore,
                EstimatedAnnualRate = annualRate,
                RecommendedTermYears = termYears,
                EvaluationResult = evaluationResult,
                Currency = currency,
                Observations = observations,
                EvaluationDate = saved.EvaluationDate
            };
        }

        public async Task<BuyAbilityResultDto?> GetLastEvaluationAsync(string clientId)
        {
            var evaluation = await _evaluationRepository.GetLatestByClientIdAsync(clientId);
            if (evaluation == null) return null;

            return new BuyAbilityResultDto
            {
                EvaluationId = evaluation.Id,
                ClientId = evaluation.ClientId,
                MonthlyGrossIncome = evaluation.MonthlyGrossIncome,
                MonthlyNetIncome = evaluation.MonthlyNetIncome,
                MonthlyDebtPayments = evaluation.MonthlyDebtPayments,
                AvailableDownPayment = evaluation.AvailableDownPayment,
                MaxMonthlyPayment = evaluation.MaxMonthlyPayment,
                MaxMortgageAmount = evaluation.MaxMortgageAmount,
                MaxPropertyPrice = evaluation.MaxPropertyPrice,
                DebtToIncomeRatio = evaluation.DebtToIncomeRatio,
                CreditScoreRating = evaluation.CreditScoreRating,
                EstimatedAnnualRate = evaluation.EstimatedAnnualRate,
                RecommendedTermYears = evaluation.RecommendedTermYears,
                EvaluationResult = evaluation.EvaluationResult,
                Currency = evaluation.Currency,
                Observations = evaluation.Observations,
                EvaluationDate = evaluation.EvaluationDate
            };
        }

        private static void ValidateInputs(BuyAbilityRequestDto request)
        {
            const decimal maxAllowedAmount = 1_000_000_000m;

            if (request.MonthlyGrossIncome < 0)
                throw new ValidationException("El ingreso mensual bruto no puede ser negativo.");

            if (request.MonthlyGrossIncome == 0)
                throw new ValidationException("El ingreso mensual bruto debe ser mayor a cero.");

            if (request.MonthlyNetIncome < 0)
                throw new ValidationException("El ingreso mensual neto no puede ser negativo.");

            if (request.MonthlyNetIncome == 0)
                throw new ValidationException("El ingreso mensual neto debe ser mayor a cero.");

            if (request.MonthlyNetIncome > request.MonthlyGrossIncome)
                throw new ValidationException("El ingreso mensual neto no puede ser mayor que el ingreso mensual bruto.");

            if (request.MonthlyDebtPayments < 0)
                throw new ValidationException("El total de deudas mensuales no puede ser negativo.");

            if (request.AvailableDownPayment < 0)
                throw new ValidationException("El pago inicial disponible no puede ser negativo.");

            if (request.MonthlyGrossIncome > maxAllowedAmount ||
                request.MonthlyNetIncome > maxAllowedAmount ||
                request.MonthlyDebtPayments > maxAllowedAmount ||
                request.AvailableDownPayment > maxAllowedAmount)
            {
                throw new ValidationException("Los montos ingresados no pueden exceder RD$ 1,000,000,000.");
            }
        }

        private static decimal CalculateDebtToIncomeRatio(decimal monthlyDebtPayments, decimal monthlyNetIncome)
        {
            if (monthlyNetIncome <= 0) return 0m;
            return (monthlyDebtPayments / monthlyNetIncome) * 100m;
        }

        private static string DetermineCreditScoreRating(decimal dti)
        {
            if (dti < 20) return "Excelente";
            if (dti <= 35) return "Bueno";
            if (dti <= 43) return "Regular";
            return "Bajo";
        }

        private static decimal DetermineEstimatedAnnualRate(string creditScoreRating)
        {
            return creditScoreRating switch
            {
                "Excelente" => 9.5m,
                "Bueno" => 11.5m,
                "Regular" => 13.5m,
                "Bajo" => 16.0m,
                _ => 16.0m
            };
        }

        private static int DetermineRecommendedTermYears(string creditScoreRating)
        {
            return creditScoreRating switch
            {
                "Excelente" => 25,
                "Bueno" => 25,
                "Regular" => 20,
                "Bajo" => 15,
                _ => 20
            };
        }

        private static decimal CalculateMaxMonthlyPayment(decimal monthlyNetIncome)
        {
            return monthlyNetIncome * 0.30m;
        }

        private static decimal CalculateMaxMortgageAmount(decimal maxMonthlyPayment, decimal annualRate, int termYears)
        {
            decimal monthlyRate = annualRate / 100m / 12m;
            int totalMonths = termYears * 12;

            if (monthlyRate == 0)
                return maxMonthlyPayment * totalMonths;

            decimal factor = (decimal)Math.Pow((double)(1 + monthlyRate), -totalMonths);
            return maxMonthlyPayment * ((1 - factor) / monthlyRate);
        }

        private static string DetermineEvaluationResult(decimal dti, decimal availableDownPayment)
        {
            if (dti < 43 && availableDownPayment > 0)
                return "Aprobado";

            if (dti < 50)
                return "Pre-Aprobado";

            return "No Aprobado";
        }

        private static string GenerateObservations(
            decimal dti,
            string creditScoreRating,
            string evaluationResult,
            decimal maxPropertyPrice,
            decimal annualRate,
            int termYears)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var formattedPrice = "RD$" + maxPropertyPrice.ToString("N2", culture);
            var formattedDti = dti.ToString("F1", culture) + "%";
            var formattedRate = annualRate.ToString("F1", culture) + "%";

            var observations = new List<string>
            {
                $"Con un nivel de endeudamiento del {formattedDti}, su perfil crediticio es considerado \"{creditScoreRating}\", con una tasa anual estimada del {formattedRate} a un plazo de {termYears} años."
            };

            if (evaluationResult == "Aprobado")
            {
                observations.Add($"Felicidades. Su capacidad de compra máxima estimada es de {formattedPrice}.");
                observations.Add("Su relación deuda/ingreso es favorable y cumple satisfactoriamente con los requisitos crediticios. Se recomienda iniciar el proceso de precalificación con una entidad bancaria.");
            }
            else if (evaluationResult == "Pre-Aprobado")
            {
                observations.Add($"Su capacidad de compra máxima estimada es de {formattedPrice}.");
                observations.Add("Se encuentra en rango de pre-aprobación. Para optimizar las condiciones del préstamo y calificar a una menor tasa de interés, se sugiere amortizar deudas existentes previo a la solicitud formal.");
            }
            else
            {
                observations.Add("Actualmente no cumple con los parámetros mínimos requeridos para una aprobación hipotecaria.");
                observations.Add("Recomendaciones clave: 1) Reducir deudas corrientes para disminuir su ratio de endeudamiento por debajo del 43%, 2) Incrementar el fondo de ahorro para el inicial, 3) Reevaluar su perfil financiero en un periodo de 3 a 6 meses.");
            }

            return string.Join(" ", observations);
        }

        private async Task DeactivatePreviousEvaluationsAsync(string clientId)
        {
            var allEvaluations = await _evaluationRepository.GetAllAsync();
            var activeEvaluations = allEvaluations
                .Where(e => e.ClientId == clientId && e.IsActive)
                .ToList();

            foreach (var evaluation in activeEvaluations)
            {
                evaluation.IsActive = false;
                await _evaluationRepository.UpdateAsync(evaluation);
            }
        }
    }
}
