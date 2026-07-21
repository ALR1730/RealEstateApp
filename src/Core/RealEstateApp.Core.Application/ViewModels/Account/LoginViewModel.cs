using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electrónico o nombre de usuario es requerido")]
        [Display(Name = "Correo o Nombre de usuario")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
