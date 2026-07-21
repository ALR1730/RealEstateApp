namespace RealEstateApp.Core.Domain.Settings
{
    /// <summary>
    /// Configuración de opciones para la generación y validación de tokens JWT.
    /// Mapeado desde appsettings.json (sección "JWTSettings").
    /// </summary>
    public class JWTSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public double DurationInMinutes { get; set; } = 60;
    }
}
