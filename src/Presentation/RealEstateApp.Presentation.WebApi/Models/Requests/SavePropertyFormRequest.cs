using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Presentation.WebApi.Extensions;

namespace RealEstateApp.Presentation.WebApi.Models.Requests
{
    /// <summary>
    /// Request HTTP multipart/form-data para crear o editar propiedades en Presentation.WebApi.
    /// Desacopla IFormFile de Core.Application (BETA).
    /// </summary>
    public class SavePropertyFormRequest
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string AgentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la propiedad es requerido")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "La moneda es requerida")]
        public string Currency { get; set; } = "DOP";

        [Required(ErrorMessage = "La cantidad de habitaciones es requerida")]
        [Range(0, 50, ErrorMessage = "Las habitaciones deben estar entre 0 y 50")]
        public int Rooms { get; set; }

        [Required(ErrorMessage = "La cantidad de baños es requerida")]
        [Range(0, 30, ErrorMessage = "Los baños deben estar entre 0 y 30")]
        public int Bathrooms { get; set; }

        [Required(ErrorMessage = "El tamaño en metros es requerido")]
        [Range(1, 100000, ErrorMessage = "El tamaño debe estar entre 1 y 100,000 m²")]
        public decimal SizeInMeters { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de propiedad es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de propiedad")]
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "El tipo de venta es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de venta")]
        public int SaleTypeId { get; set; }

        [Required(ErrorMessage = "La provincia es requerida")]
        public int? ProvinceId { get; set; }

        [Required(ErrorMessage = "El municipio es requerido")]
        public int? MunicipalityId { get; set; }

        [Required(ErrorMessage = "El sector o barrio es requerido")]
        [StringLength(100, ErrorMessage = "El sector no puede exceder 100 caracteres")]
        public string? Sector { get; set; }

        [Required(ErrorMessage = "La dirección completa es requerida")]
        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string? FullAddress { get; set; }

        [Required(ErrorMessage = "La latitud es requerida")]
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "La longitud es requerida")]
        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
        public double Longitude { get; set; }

        [Url(ErrorMessage = "Ingrese una URL válida para el video")]
        [StringLength(500, ErrorMessage = "La URL del video no puede exceder 500 caracteres")]
        public string? VideoUrl { get; set; }

        [Url(ErrorMessage = "Ingrese una URL válida para el tour 360")]
        [StringLength(500, ErrorMessage = "La URL del tour no puede exceder 500 caracteres")]
        public string? Tour360Url { get; set; }

        [StringLength(50, ErrorMessage = "El ID de Matterport no puede exceder 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_-]*$", ErrorMessage = "El ID de Matterport solo puede contener letras, números, guiones y guiones bajos")]
        public string? MatterportModelId { get; set; }

        [Required(ErrorMessage = "El monto de separación es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto de separación debe ser mayor a cero")]
        public decimal MontoSeparacion { get; set; }

        [Required(ErrorMessage = "El porcentaje inicial es requerido")]
        [Range(1, 100, ErrorMessage = "El porcentaje debe estar entre 1 y 100")]
        public int PorcentajeInicialRequerido { get; set; }

        public bool IsFinanciable { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? FeaturedUntil { get; set; }

        public List<IFormFile>? Files { get; set; }
        public List<int>? ImprovementIds { get; set; }
        public List<string>? ExistingImages { get; set; }

        public SavePropertyViewModel ToViewModel() => new()
        {
            Id = Id,
            Code = Code,
            AgentId = AgentId,
            Name = Name,
            Price = Price,
            Currency = Currency,
            Rooms = Rooms,
            Bathrooms = Bathrooms,
            SizeInMeters = SizeInMeters,
            Description = Description,
            PropertyTypeId = PropertyTypeId,
            SaleTypeId = SaleTypeId,
            ProvinceId = ProvinceId,
            MunicipalityId = MunicipalityId,
            Sector = Sector,
            FullAddress = FullAddress,
            Latitude = Latitude,
            Longitude = Longitude,
            VideoUrl = VideoUrl,
            Tour360Url = Tour360Url,
            MatterportModelId = MatterportModelId,
            MontoSeparacion = MontoSeparacion,
            PorcentajeInicialRequerido = PorcentajeInicialRequerido,
            IsFinanciable = IsFinanciable,
            IsFeatured = IsFeatured,
            FeaturedUntil = FeaturedUntil,
            Files = Files.ToFileUploadDtos(),
            ImprovementIds = ImprovementIds,
            ExistingImages = ExistingImages
        };
    }
}
