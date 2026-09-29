using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Review;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Sistema de reseñas y calificaciones a agentes (Ítem 2.9).
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IAccountService _accountService;

        public ReviewService(
            IReviewRepository reviewRepository,
            IAccountService accountService)
        {
            _reviewRepository = reviewRepository;
            _accountService = accountService;
        }

        public async Task SubmitReviewAsync(SaveAgentReviewViewModel vm, string clienteId)
        {
            var hasTransaction = await _reviewRepository
                .HasCompletedTransactionAsync(clienteId, vm.AgentId, vm.PropertyId);
            if (!hasTransaction)
            {
                throw new ValidationException("Solo puedes calificar a un agente después de completar una transacción en una de sus propiedades.");
            }

            var exists = await _reviewRepository.ExistsAsync(clienteId, vm.AgentId, vm.PropertyId);
            if (exists)
            {
                throw new ValidationException("Ya has calificado a este agente por esta propiedad.");
            }

            var review = new AgentReview
            {
                AgentId = vm.AgentId,
                ClienteId = clienteId,
                PropertyId = vm.PropertyId,
                Rating = vm.Rating,
                Comment = vm.Comment ?? string.Empty
            };

            await _reviewRepository.AddAsync(review);
        }

        public async Task<bool> CanReviewAsync(string clienteId, string agentId, int propertyId)
        {
            if (await _reviewRepository.ExistsAsync(clienteId, agentId, propertyId)) return false;
            return await _reviewRepository.HasCompletedTransactionAsync(clienteId, agentId, propertyId);
        }

        public async Task<bool> HasReviewedAsync(string clienteId, string agentId, int propertyId)
        {
            return await _reviewRepository.ExistsAsync(clienteId, agentId, propertyId);
        }

        public async Task<List<AgentReviewViewModel>> GetReviewsByAgentAsync(string agentId)
        {
            var entities = await _reviewRepository.GetByAgentIdAsync(agentId);
            return await MapToViewModelsAsync(entities);
        }

        public async Task<AgentReviewSummaryViewModel> GetAgentReviewSummaryAsync(string agentId)
        {
            var entities = await _reviewRepository.GetByAgentIdAsync(agentId);
            var count = entities.Count;

            return new AgentReviewSummaryViewModel
            {
                ReviewCount = count,
                AverageRating = count > 0 ? Math.Round(entities.Average(r => r.Rating), 1) : 0,
                FiveStars = entities.Count(r => r.Rating == 5),
                FourStars = entities.Count(r => r.Rating == 4),
                ThreeStars = entities.Count(r => r.Rating == 3),
                TwoStars = entities.Count(r => r.Rating == 2),
                OneStar = entities.Count(r => r.Rating == 1)
            };
        }

        public async Task<List<AgentReviewViewModel>> GetAllAsync()
        {
            var entities = await _reviewRepository.GetAllWithDetailsAsync();
            return await MapToViewModelsAsync(entities);
        }

        private async Task<List<AgentReviewViewModel>> MapToViewModelsAsync(List<AgentReview> entities)
        {
            // Batch lookup: 1 sola query para todos los clientes, eliminando el N+1
            var clientIds = entities.Select(r => r.ClienteId).Distinct();
            var userDict = await _accountService.GetUsersByIdsAsync(clientIds);

            var result = new List<AgentReviewViewModel>();
            foreach (var r in entities)
            {
                var vm = new AgentReviewViewModel
                {
                    Id = r.Id,
                    AgentId = r.AgentId,
                    ClienteId = r.ClienteId,
                    PropertyId = r.PropertyId,
                    PropertyCode = r.Property?.Code,
                    PropertyName = r.Property?.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Created = r.Created
                };

                if (userDict.TryGetValue(r.ClienteId, out var cliente))
                {
                    var fullName = $"{cliente.FirstName} {cliente.LastName}".Trim();
                    vm.ClienteName = !string.IsNullOrEmpty(fullName) ? fullName : (cliente.UserName ?? "Cliente");
                    vm.ClienteEmail = cliente.Email;
                }

                result.Add(vm);
            }
            return result;
        }
    }
}