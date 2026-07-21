namespace RealEstateApp.Core.Application.ViewModels.Favorite
{
    /// <summary>
    /// ViewModel para mostrar inmuebles favoritos del cliente.
    /// Incluye datos resueltos de la propiedad para evitar consultas adicionales.
    /// </summary>
    public class FavoriteViewModel
    {
        public int Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;

        // Datos de la propiedad
        public int PropertyId { get; set; }
        public string PropertyCode { get; set; } = string.Empty;
        public decimal PropertyPrice { get; set; }
        public string PropertyTypeName { get; set; } = string.Empty;
        public int PropertyRooms { get; set; }
        public int PropertyBathrooms { get; set; }
        public decimal PropertySizeInMeters { get; set; }
        public string PropertyStatus { get; set; } = string.Empty;
        public string? PropertyMainImage { get; set; }
    }
}
