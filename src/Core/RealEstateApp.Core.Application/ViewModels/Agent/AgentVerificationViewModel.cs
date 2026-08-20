using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Application.ViewModels.Agent
{
    public class AgentVerificationViewModel
    {
        public int Id { get; set; }
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? AgentEmail { get; set; }
        public string? AgentPhone { get; set; }

        [Required(ErrorMessage = "El número de cédula es obligatorio.")]
        [Display(Name = "Cédula de Identidad")]
        public string Cedula { get; set; } = string.Empty;

        [Display(Name = "Foto Frontal de la Cédula")]
        public IFormFile? FrontImageFile { get; set; }

        [Display(Name = "Foto Posterior de la Cédula")]
        public IFormFile? BackImageFile { get; set; }

        public string? FrontImageUrl { get; set; }
        public string? BackImageUrl { get; set; }

        public string Status { get; set; } = VerificationStatus.Pending;
        public string? RejectionReason { get; set; }

        public DateTime? Created { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedByAdminId { get; set; }
        public string? ReviewedByAdminName { get; set; }

        public bool IsVerified => Status == VerificationStatus.Approved;
        public bool IsPending => Status == VerificationStatus.Pending;
        public bool IsRejected => Status == VerificationStatus.Rejected;
    }
}
