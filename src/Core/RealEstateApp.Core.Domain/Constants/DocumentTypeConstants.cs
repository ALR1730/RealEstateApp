namespace RealEstateApp.Core.Domain.Constants
{
    /// <summary>
    /// Tipos de documento que puede gestionar una propiedad.
    /// </summary>
    public static class DocumentTypeConstants
    {
        public const string Titulo = "Título de propiedad";
        public const string ContratoVenta = "Contrato de compra-venta";
        public const string ContratoAlquiler = "Contrato de alquiler";
        public const string CartaPreAprobacion = "Carta de pre-aprobación";
        public const string Certificacion = "Certificación de registro";
        public const string Otro = "Otro";

        public static readonly string[] All = new[]
        {
            Titulo,
            ContratoVenta,
            ContratoAlquiler,
            CartaPreAprobacion,
            Certificacion,
            Otro
        };

        public static bool IsValid(string value)
        {
            return System.Array.IndexOf(All, value) >= 0;
        }
    }
}