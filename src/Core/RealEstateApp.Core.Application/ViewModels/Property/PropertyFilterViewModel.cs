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

        [Display(Name = "Provincia")]
        public int? ProvinceId { get; set; }

        [Display(Name = "Municipio")]
        public int? MunicipalityId { get; set; }

        [StringLength(100, ErrorMessage = "El sector no puede exceder 100 caracteres")]
        [Display(Name = "Sector / Barrio")]
        public string? Sector { get; set; }

        [Display(Name = "Solo Listados Destacados")]
        public bool? OnlyFeatured { get; set; }

        [Display(Name = "Solo Agentes Verificados")]
        public bool? OnlyVerifiedAgents { get; set; }

        [Display(Name = "Solo con Financiamiento")]
        public bool? OnlyFinanciable { get; set; }

        [Display(Name = "Solo con Recorrido Virtual / Tour 3D")]
        public bool? OnlyWithVirtualTour { get; set; }

        [Range(0, 100000, ErrorMessage = "El tamaño mínimo debe estar entre 0 y 100,000 m²")]
        [Display(Name = "Tamaño Mínimo (m²)")]
        public decimal? MinSizeInMeters { get; set; }

        [Range(0, 100000, ErrorMessage = "El tamaño máximo debe estar entre 0 y 100,000 m²")]
        [Display(Name = "Tamaño Máximo (m²)")]
        public decimal? MaxSizeInMeters { get; set; }

        [Display(Name = "Mejoras / Amenidades")]
        public List<int>? ImprovementIds { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor o igual a 1")]
        [Display(Name = "Número de Página")]
        public int? PageNumber { get; set; }

        [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
        [Display(Name = "Tamaño de Página")]
        public int? PageSize { get; set; }

        [Display(Name = "Latitud de Usuario")]
        public double? UserLat { get; set; }

        [Display(Name = "Longitud de Usuario")]
        public double? UserLng { get; set; }

        [Display(Name = "Radio Máximo (Km)")]
        public double? MaxDistanceKm { get; set; }

        // ---- Datos auxiliares para poblar los dropdowns y checkboxes ----
        public List<PropertyTypeViewModel>? PropertyTypes { get; set; }
        public List<SaleTypeViewModel>? SaleTypes { get; set; }
        public List<ProvinceDropdownViewModel>? Provinces { get; set; }
        public List<MunicipalityDropdownViewModel>? Municipalities { get; set; }
        public List<ImprovementViewModel>? AvailableImprovements { get; set; }
    }

    public class ProvinceDropdownViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IsoCode { get; set; } = string.Empty;
    }

    public class MunicipalityDropdownViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProvinceId { get; set; }
    }
}
