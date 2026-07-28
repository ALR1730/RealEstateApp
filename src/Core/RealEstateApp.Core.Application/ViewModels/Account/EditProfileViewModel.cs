using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Core.Application.ViewModels.Account
{
    /// <summary>
    /// ViewModel para la visualización y edición del perfil de usuario.
    /// </summary>
    public class EditProfileViewModel
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Ingrese un número telefónico válido")]
        [Display(Name = "Teléfono de Contacto")]
        public string? Phone { get; set; }

        [Display(Name = "Foto de Perfil Actual")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Cambiar Foto de Perfil")]
        public IFormFile? ProfilePictureFile { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual (Requerida si desea cambiar contraseña)")]
        public string? CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña (Opcional)")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Nueva Contraseña")]
        public string? ConfirmNewPassword { get; set; }

        public string Role { get; set; } = string.Empty;
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
