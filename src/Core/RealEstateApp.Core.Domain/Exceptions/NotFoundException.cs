using System;

namespace RealEstateApp.Core.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando una entidad o recurso solicitado no existe.
    /// </summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException() : base("El recurso solicitado no fue encontrado.") { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string name, object key) : base($"No se encontró la entidad '{name}' con la clave '{key}'.") { }
    }
}
