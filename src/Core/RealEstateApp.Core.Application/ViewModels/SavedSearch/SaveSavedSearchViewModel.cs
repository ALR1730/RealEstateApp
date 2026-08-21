using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.SavedSearch
{
    public class SaveSavedSearchViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Debes ingresar un nombre descriptivo para tu búsqueda.")]
        [StringLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres.")]
        [Display(Name = "Nombre de la Búsqueda")]
        public string Name { get; set; } = string.Empty;

        // Criterios
        public int? PropertyTypeId { get; set; }
        public int? SaleTypeId { get; set; }
        public int? ProvinceId { get; set; }
        public int? MunicipalityId { get; set; }
        public string? Sector { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinRooms { get; set; }
        public int? MaxRooms { get; set; }
        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }
        public decimal? MinSizeInMeters { get; set; }
        public decimal? MaxSizeInMeters { get; set; }
        public bool? OnlyFinanciable { get; set; }
        public bool? OnlyWithVirtualTour { get; set; }

        // Alertas
        [Display(Name = "Recibir alertas por correo electrónico")]
        public bool EmailAlertsEnabled { get; set; } = true;

        [Display(Name = "Recibir alertas dentro de la plataforma")]
        public bool InAppAlertsEnabled { get; set; } = true;
    }
}
