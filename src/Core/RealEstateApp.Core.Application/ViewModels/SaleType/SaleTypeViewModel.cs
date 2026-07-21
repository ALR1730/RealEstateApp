namespace RealEstateApp.Core.Application.ViewModels.SaleType
{
    /// <summary>
    /// ViewModel de lectura para tipos de venta.
    /// </summary>
    public class SaleTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad de propiedades que usan este tipo de venta.
        /// </summary>
        public int PropertiesCount { get; set; }
    }
}
