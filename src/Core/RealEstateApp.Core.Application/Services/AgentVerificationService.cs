using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class AgentVerificationService : IAgentVerificationService
    {
        private readonly IAgentVerificationRepository _verificationRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<IdentityUser> _userManager;

        public AgentVerificationService(
            IAgentVerificationRepository verificationRepository,
            IFileStorageService fileStorageService,
            UserManager<IdentityUser> userManager)
        {
            _verificationRepository = verificationRepository;
            _fileStorageService = fileStorageService;
            _userManager = userManager;
        }

        public async Task<AgentVerificationViewModel?> GetByAgentIdAsync(string agentId)
        {
            var entity = await _verificationRepository.GetByAgentIdAsync(agentId);
            if (entity == null) return null;

            var vm = MapToViewModel(entity);
            await PopulateUserDetails(vm, entity.AgentId, entity.ReviewedByAdminId);
            return vm;
        }

        public async Task<List<AgentVerificationViewModel>> GetAllAsync()
        {
            var entities = await _verificationRepository.GetAllAsync();
            var result = new List<AgentVerificationViewModel>();

            foreach (var entity in entities)
            {
                var vm = MapToViewModel(entity);
                await PopulateUserDetails(vm, entity.AgentId, entity.ReviewedByAdminId);
                result.Add(vm);
            }

            return result;
        }

        public async Task<List<AgentVerificationViewModel>> GetPendingAsync()
        {
            var entities = await _verificationRepository.GetPendingVerificationsAsync();
            var result = new List<AgentVerificationViewModel>();

            foreach (var entity in entities)
            {
                var vm = MapToViewModel(entity);
                await PopulateUserDetails(vm, entity.AgentId, entity.ReviewedByAdminId);
                result.Add(vm);
            }

            return result;
        }

        public async Task<bool> SubmitVerificationAsync(AgentVerificationViewModel vm)
        {
            var agentId = vm.AgentId ?? string.Empty;
            var existing = await _verificationRepository.GetByAgentIdAsync(agentId);

            string? frontUrl = existing?.CedulaFrontImageUrl;
            string? backUrl = existing?.CedulaBackImageUrl;

            if (vm.FrontImageFile != null && vm.FrontImageFile.Length > 0)
            {
                using var stream = vm.FrontImageFile.OpenReadStream();
                frontUrl = await _fileStorageService.UploadFileAsync(stream, vm.FrontImageFile.FileName, "verifications");
            }

            if (vm.BackImageFile != null && vm.BackImageFile.Length > 0)
            {
                using var stream = vm.BackImageFile.OpenReadStream();
                backUrl = await _fileStorageService.UploadFileAsync(stream, vm.BackImageFile.FileName, "verifications");
            }

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

        private async Task PopulateUserDetails(AgentVerificationViewModel vm, string agentId, string? adminId)
        {
            var agent = await _userManager.FindByIdAsync(agentId);
            if (agent != null)
            {
                var claims = await _userManager.GetClaimsAsync(agent);
                var firstName = claims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
                var lastName = claims.FirstOrDefault(c => c.Type == "LastName")?.Value;
                var fullName = $"{firstName} {lastName}".Trim();

                vm.AgentName = !string.IsNullOrEmpty(fullName) ? fullName : (agent.UserName ?? "Agente");
                vm.AgentEmail = agent.Email ?? string.Empty;
                vm.AgentPhone = agent.PhoneNumber ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(adminId))
            {
                var admin = await _userManager.FindByIdAsync(adminId);
                if (admin != null)
                {
                    var adminClaims = await _userManager.GetClaimsAsync(admin);
                    var adminFirst = adminClaims.FirstOrDefault(c => c.Type == "FirstName")?.Value;
                    var adminLast = adminClaims.FirstOrDefault(c => c.Type == "LastName")?.Value;
                    var adminFull = $"{adminFirst} {adminLast}".Trim();

                    vm.ReviewedByAdminName = !string.IsNullOrEmpty(adminFull) ? adminFull : (admin.UserName ?? "Administrador");
                }
            }
        }
    }
}
