using System;

namespace RealEstateApp.Core.Domain.Exceptions
{
    /// <summary>
    /// Excepción base de dominio para la aplicación.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException() : base() { }
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
