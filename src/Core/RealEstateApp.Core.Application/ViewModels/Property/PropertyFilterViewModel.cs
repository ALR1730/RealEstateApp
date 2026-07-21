using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Property
{
    /// <summary>
    /// ViewModel para el formulario de filtros combinados del buscador de propiedades.
    /// Todos los campos son opcionales para permitir filtros parciales.
    /// </summary>
    public class PropertyFilterViewModel
    {
        [StringLength(6, ErrorMessage = "El código debe tener máximo 6 caracteres")]
        [Display(Name = "Código")]
        public string? Code { get; set; }

        [Display(Name = "Tipo de Propiedad")]
        public int? PropertyTypeId { get; set; }

        [Display(Name = "Tipo de Venta")]
        public int? SaleTypeId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio mínimo no puede ser negativo")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio Mínimo")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio máximo no puede ser negativo")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio Máximo")]
        public decimal? MaxPrice { get; set; }

        [Range(0, 50, ErrorMessage = "Las habitaciones deben estar entre 0 y 50")]
        [Display(Name = "Habitaciones Mínimas")]
        public int? MinRooms { get; set; }

        [Range(0, 50, ErrorMessage = "Las habitaciones deben estar entre 0 y 50")]
        [Display(Name = "Habitaciones Máximas")]
        public int? MaxRooms { get; set; }

        [Range(0, 30, ErrorMessage = "Los baños deben estar entre 0 y 30")]
        [Display(Name = "Baños Mínimos")]
        public int? MinBathrooms { get; set; }

        [Range(0, 30, ErrorMessage = "Los baños deben estar entre 0 y 30")]
        [Display(Name = "Baños Máximos")]
        public int? MaxBathrooms { get; set; }

        [Display(Name = "Agente")]
        public string? AgentId { get; set; }

        // ---- Datos auxiliares para poblar los dropdowns ----
        public List<PropertyTypeViewModel>? PropertyTypes { get; set; }
        public List<SaleTypeViewModel>? SaleTypes { get; set; }
    }
}
