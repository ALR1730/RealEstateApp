using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Domain.Constants;

namespace RealEstateApp.Core.Application.ViewModels.Agent
{
    public class AgentVerificationViewModel
    {
        public int Id { get; set; }
        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string AgentEmail { get; set; } = string.Empty;
        public string AgentPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es requerida")]
        [StringLength(15, MinimumLength = 11, ErrorMessage = "El formato de cédula debe tener 11 dígitos")]
        [Display(Name = "Cédula de Identidad (ej. 001-1234567-8)")]
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
