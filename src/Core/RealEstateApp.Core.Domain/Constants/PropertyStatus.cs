namespace RealEstateApp.Core.Domain.Constants
{
    /// <summary>
    /// Constantes centralizadas para los estados de una propiedad.
    /// Evita el uso de cadenas mágicas dispersas en la solución.
    /// </summary>
    public static class PropertyStatus
    {
        public const string Available = "Disponible";
        public const string Reserved = "Reservada";
        public const string Sold = "Vendida";
    }
}
