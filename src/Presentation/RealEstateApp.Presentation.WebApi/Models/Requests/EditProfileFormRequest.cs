using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Application.ViewModels.Account;
using RealEstateApp.Presentation.WebApi.Extensions;

namespace RealEstateApp.Presentation.WebApi.Models.Requests
{
    public class EditProfileFormRequest
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        public string UserName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Ingrese un número telefónico válido")]
        public string? Phone { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public IFormFile? ProfilePictureFile { get; set; }
        public IFormFile? Photo { get; set; }

        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres")]
        public string? NewPassword { get; set; }

        [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
        public string? ConfirmNewPassword { get; set; }

        public EditProfileViewModel ToViewModel(string userId)
        {
            var picture = ProfilePictureFile ?? Photo;

            return new EditProfileViewModel
            {
                Id = userId,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                UserName = UserName,
                Phone = Phone,
                ProfilePictureUrl = ProfilePictureUrl,
                ProfilePictureFile = picture.ToFileUploadDto(),
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmNewPassword = ConfirmNewPassword
            };
        }
    }
}
