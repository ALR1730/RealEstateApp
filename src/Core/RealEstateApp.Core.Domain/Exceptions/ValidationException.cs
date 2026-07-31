using System;

namespace RealEstateApp.Core.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando fallan las reglas de negocio o validaciones de dominio.
    /// </summary>
    public class ValidationException : DomainException
    {
        public ValidationException() : base("Han ocurrido uno o más errores de validación de dominio.") { }
        public ValidationException(string message) : base(message) { }
    }
}
