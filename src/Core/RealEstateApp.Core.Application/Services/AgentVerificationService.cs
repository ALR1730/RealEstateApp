using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.DTOs.Account;
using RealEstateApp.Core.Application.DTOs.Common;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Exceptions;

namespace RealEstateApp.Core.Application.Services
{
    public class AgentVerificationService : IAgentVerificationService
    {
        private readonly IAgentVerificationRepository _verificationRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAccountService _accountService;
        private readonly IFileValidationService _fileValidationService;

        public AgentVerificationService(
            IAgentVerificationRepository verificationRepository,
            IFileStorageService fileStorageService,
            IAccountService accountService,
            IFileValidationService? fileValidationService = null)
        {
            _verificationRepository = verificationRepository;
            _fileStorageService = fileStorageService;
            _accountService = accountService;
            _fileValidationService = fileValidationService ?? new FileValidationService();
        }

        public async Task<AgentVerificationViewModel?> GetByAgentIdAsync(string agentId)
        {
            var entity = await _verificationRepository.GetByAgentIdAsync(agentId);
            if (entity == null) return null;

            var vm = MapToViewModel(entity);

            // Batch lookup con sólo los IDs necesarios para este registro individual
            var ids = new List<string> { entity.AgentId };
            if (!string.IsNullOrEmpty(entity.ReviewedByAdminId))
                ids.Add(entity.ReviewedByAdminId);

            var userDict = await _accountService.GetUsersByIdsAsync(ids);
            PopulateUserDetails(vm, entity.AgentId, entity.ReviewedByAdminId, userDict);
            return vm;
        }

        public async Task<List<AgentVerificationViewModel>> GetAllAsync()
        {
            var entities = await _verificationRepository.GetAllAsync();
            return await MapWithBatchLookupAsync(entities);
        }

        public async Task<List<AgentVerificationViewModel>> GetPendingAsync()
        {
            var entities = await _verificationRepository.GetPendingVerificationsAsync();
            return await MapWithBatchLookupAsync(entities);
        }

        public async Task<bool> SubmitVerificationAsync(AgentVerificationViewModel vm)
        {
            var agentId = vm.AgentId ?? string.Empty;
            var existing = await _verificationRepository.GetByAgentIdAsync(agentId);

            string? frontUrl = await ValidateAndUploadVerificationImageAsync(vm.FrontImageFile, existing?.CedulaFrontImageUrl);
            string? backUrl = await ValidateAndUploadVerificationImageAsync(vm.BackImageFile, existing?.CedulaBackImageUrl);

            if (existing != null)
            {
                existing.Cedula = vm.Cedula;
                if (!string.IsNullOrEmpty(frontUrl)) existing.CedulaFrontImageUrl = frontUrl;
                if (!string.IsNullOrEmpty(backUrl)) existing.CedulaBackImageUrl = backUrl;
                existing.Status = VerificationStatus.Pending;
                existing.RejectionReason = null;
                existing.ReviewedAt = null;
                existing.ReviewedByAdminId = null;

                await _verificationRepository.UpdateAsync(existing);
            }
            else
            {
                var newEntity = new AgentVerification
                {
                    AgentId = agentId,
                    Cedula = vm.Cedula,
                    CedulaFrontImageUrl = frontUrl ?? string.Empty,
                    CedulaBackImageUrl = backUrl ?? string.Empty,
                    Status = VerificationStatus.Pending
                };

                await _verificationRepository.AddAsync(newEntity);
            }

            return true;
        }

        public async Task<bool> ReviewVerificationAsync(int id, bool approved, string? rejectionReason, string adminId)
        {
            var entity = await _verificationRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Status = approved ? VerificationStatus.Approved : VerificationStatus.Rejected;
            entity.RejectionReason = approved ? null : rejectionReason;
            entity.ReviewedByAdminId = adminId;
            entity.ReviewedAt = DateTime.UtcNow;

            await _verificationRepository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> IsAgentVerifiedAsync(string agentId)
        {
            var entity = await _verificationRepository.GetByAgentIdAsync(agentId);
            return entity != null && entity.Status == VerificationStatus.Approved;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Mapea una lista de entidades haciendo un único batch lookup para todos
        /// los agentes y admins implicados — de N*4 queries a exactamente 1 query.
        /// </summary>
        private async Task<List<AgentVerificationViewModel>> MapWithBatchLookupAsync(
            List<AgentVerification> entities)
        {
            // Recopilar todos los IDs únicos en un solo conjunto
            var allIds = entities
                .SelectMany(e => new[] { e.AgentId, e.ReviewedByAdminId })
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()!;

            var userDict = await _accountService.GetUsersByIdsAsync(allIds!);

            var result = new List<AgentVerificationViewModel>();
            foreach (var entity in entities)
            {
                var vm = MapToViewModel(entity);
                PopulateUserDetails(vm, entity.AgentId, entity.ReviewedByAdminId, userDict);
                result.Add(vm);
            }

            return result;
        }

        private static AgentVerificationViewModel MapToViewModel(AgentVerification entity)
        {
            return new AgentVerificationViewModel
            {
                Id = entity.Id,
                AgentId = entity.AgentId,
                Cedula = entity.Cedula,
                FrontImageUrl = entity.CedulaFrontImageUrl,
                BackImageUrl = entity.CedulaBackImageUrl,
                Status = entity.Status,
                RejectionReason = entity.RejectionReason,
                Created = entity.Created,
                ReviewedAt = entity.ReviewedAt,
                ReviewedByAdminId = entity.ReviewedByAdminId
            };
        }

        /// <summary>
        /// Puebla los datos de usuario desde un diccionario pre-cargado (sin queries adicionales).
        /// </summary>
        private static void PopulateUserDetails(
            AgentVerificationViewModel vm,
            string agentId,
            string? adminId,
            Dictionary<string, AccountUserDto> userDict)
        {
            if (userDict.TryGetValue(agentId, out var agent))
            {
                var fullName = $"{agent.FirstName} {agent.LastName}".Trim();
                vm.AgentName = !string.IsNullOrEmpty(fullName) ? fullName : (agent.UserName ?? "Agente");
                vm.AgentEmail = agent.Email;
                vm.AgentPhone = agent.PhoneNumber ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(adminId) && userDict.TryGetValue(adminId, out var admin))
            {
                var adminFull = $"{admin.FirstName} {admin.LastName}".Trim();
                vm.ReviewedByAdminName = !string.IsNullOrEmpty(adminFull) ? adminFull : (admin.UserName ?? "Administrador");
            }
        }

        private async Task<string?> ValidateAndUploadVerificationImageAsync(FileUploadDto? file, string? currentUrl)
        {
            if (file == null || file.Length <= 0)
            {
                return currentUrl;
            }

            if (file.Length > FileValidationService.MaxVerificationImageBytes)
            {
                throw new ValidationException("La imagen de verificación no puede superar los 5 MB.");
            }

            using var stream = file.Content;
            await _fileValidationService.ValidateAsync(stream, file.FileName, file.ContentType, FileUploadPolicy.VerificationImage);
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return await _fileStorageService.UploadFileAsync(stream, file.FileName, "verifications");
        }
    }
}
