using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Presentation.WebApi.Extensions;

namespace RealEstateApp.Presentation.WebApi.Models.Requests
{
    public class SubmitVerificationFormRequest
    {
        [Required(ErrorMessage = "El número de cédula es obligatorio.")]
        public string Cedula { get; set; } = string.Empty;

        public IFormFile? FrontImageFile { get; set; }
        public IFormFile? BackImageFile { get; set; }

        // Aliases aceptados desde el frontend SPA
        public IFormFile? CedulaFrontImage { get; set; }
        public IFormFile? CedulaBackImage { get; set; }

        public AgentVerificationViewModel ToViewModel(string agentId)
        {
            var frontFile = FrontImageFile ?? CedulaFrontImage;
            var backFile = BackImageFile ?? CedulaBackImage;

            return new AgentVerificationViewModel
            {
                AgentId = agentId,
                Cedula = Cedula,
                FrontImageFile = frontFile.ToFileUploadDto(),
                BackImageFile = backFile.ToFileUploadDto()
            };
        }
    }
}
