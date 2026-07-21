using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.DTOs.Improvement
{
    /// <summary>
    /// DTO de mejora para exposición en la API REST.
    /// </summary>
    public class ImprovementDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Description { get; set; } = string.Empty;
    }
}
