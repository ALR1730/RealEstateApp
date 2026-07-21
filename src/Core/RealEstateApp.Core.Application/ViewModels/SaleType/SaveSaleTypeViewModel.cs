using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.SaleType
{
    /// <summary>
    /// ViewModel para formulario de creación/edición de tipo de venta.
    /// </summary>
    public class SaveSaleTypeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string Description { get; set; } = string.Empty;
    }
}
