using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RealEstateApp.Core.Application.ViewModels.Property
{
    /// <summary>
    /// ViewModel para formulario de creación/edición de propiedad (Agente).
    /// Contiene validaciones DataAnnotations para el lado servidor y cliente.
    /// </summary>
    public class SavePropertyViewModel
    {
        public int Id { get; set; }

        /// <summary>
        /// Código único de 6 dígitos. Se autogenera al crear, no editable por el usuario.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// ID del agente propietario de la propiedad.
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la propiedad es requerido")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres")]
        [Display(Name = "Nombre de la Propiedad")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
        [DataType(DataType.Currency)]
        [Display(Name = "Precio")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "La cantidad de habitaciones es requerida")]
        [Range(0, 50, ErrorMessage = "Las habitaciones deben estar entre 0 y 50")]
        [Display(Name = "Habitaciones")]
        public int Rooms { get; set; }

        [Required(ErrorMessage = "La cantidad de baños es requerida")]
        [Range(0, 30, ErrorMessage = "Los baños deben estar entre 0 y 30")]
        [Display(Name = "Baños")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "El tamaño en metros es requerido")]
        [Range(1, 100000, ErrorMessage = "El tamaño debe estar entre 1 y 100,000 m²")]
        [Display(Name = "Tamaño (m²)")]
        public decimal SizeInMeters { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de propiedad es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de propiedad")]
        [Display(Name = "Tipo de Propiedad")]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "El tipo de venta es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de venta")]
        [Display(Name = "Tipo de Venta")]
        public int SaleTypeId { get; set; }

        [Required(ErrorMessage = "La latitud es requerida")]
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90")]
        [Display(Name = "Latitud")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "La longitud es requerida")]
        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
        [Display(Name = "Longitud")]
        public double Longitude { get; set; }

        [Url(ErrorMessage = "Ingrese una URL válida para el video")]
        [StringLength(500, ErrorMessage = "La URL del video no puede exceder 500 caracteres")]
        [Display(Name = "URL del Video")]
        public string? VideoUrl { get; set; }

        [Url(ErrorMessage = "Ingrese una URL válida para el tour 360")]
        [StringLength(500, ErrorMessage = "La URL del tour no puede exceder 500 caracteres")]
        [Display(Name = "URL del Tour 360°")]
        public string? Tour360Url { get; set; }

        [Required(ErrorMessage = "El monto de separación es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de separación debe ser mayor a cero")]
        [DataType(DataType.Currency)]
        [Display(Name = "Monto de Separación")]
        public decimal MontoSeparacion { get; set; }

        [Required(ErrorMessage = "El porcentaje inicial es requerido")]
        [Range(1, 100, ErrorMessage = "El porcentaje debe estar entre 1 y 100")]
        [Display(Name = "Porcentaje Inicial Requerido (%)")]
        public int PorcentajeInicialRequerido { get; set; }

        [Display(Name = "¿Pre-investigación para Financiamiento aprobada?")]
        public bool IsFinanciable { get; set; }

        /// <summary>
        /// Archivos de imagen subidos por el formulario.
        /// Se permiten hasta 15 imágenes por propiedad.
        /// </summary>
        [Display(Name = "Imágenes del Inmueble (máx. 15)")]
        public List<IFormFile>? Files { get; set; }

        /// <summary>
        /// IDs de mejoras seleccionadas (checkbox múltiple).
        /// </summary>
        [Display(Name = "Mejoras")]
        public List<int>? ImprovementIds { get; set; }

        // ---- Datos auxiliares para poblar dropdowns/checkboxes en la vista ----

        /// <summary>
        /// Lista de tipos de propiedad para el dropdown.
        /// </summary>
        public List<PropertyTypeViewModel>? PropertyTypes { get; set; }

        /// <summary>
        /// Lista de tipos de venta para el dropdown.
        /// </summary>
        public List<SaleTypeViewModel>? SaleTypes { get; set; }

        /// <summary>
        /// Lista de mejoras disponibles para los checkboxes.
        /// </summary>
        public List<ImprovementViewModel>? Improvements { get; set; }

        /// <summary>
        /// URLs de imágenes ya subidas (para edición).
        /// </summary>
        public List<string>? ExistingImages { get; set; }
    }

    // ViewModels auxiliares usados en los dropdowns del formulario
    // (referenciados aquí para completitud; se definen en sus propias carpetas)
    public class PropertyTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SaleTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ImprovementViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
