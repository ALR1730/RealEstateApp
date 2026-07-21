using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.DTOs.PropertyType
{
    /// <summary>
    /// DTO de tipo de propiedad para exposición en la API REST.
    /// </summary>
    public class PropertyTypeDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad de propiedades asociadas a este tipo.
        /// </summary>
        public int PropertiesCount { get; set; }
    }
}
