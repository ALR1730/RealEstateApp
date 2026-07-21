using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.PropertyType
{
    /// <summary>
    /// ViewModel de lectura para tipos de propiedad.
    /// </summary>
    public class PropertyTypeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad de propiedades que usan este tipo.
        /// </summary>
        public int PropertiesCount { get; set; }
    }
}
