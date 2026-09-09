using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Review
{
    public class SaveAgentReviewViewModel
    {
        public string AgentId { get; set; } = string.Empty;
        public int PropertyId { get; set; }

        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas")]
        [Required(ErrorMessage = "La calificación es requerida")]
        [Display(Name = "Calificación")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
        [Display(Name = "Comentario")]
        public string Comment { get; set; } = string.Empty;
    }
}
