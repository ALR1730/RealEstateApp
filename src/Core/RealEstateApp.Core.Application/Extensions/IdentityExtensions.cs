using System;
using Microsoft.AspNetCore.Identity;

namespace RealEstateApp.Core.Application.Extensions
{
    /// <summary>
    /// Métodos de extensión centralizados para IdentityUser.
    /// Evita duplicación de lógica para la verificación de estado activo.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Determina si un usuario de Identity se encuentra activo en el sistema.
        /// Un usuario está activo si no tiene el bloqueo habilitado, no tiene fecha de fin de bloqueo,
        /// o la fecha de fin de bloqueo ya expiró.
        /// </summary>
        public static bool IsActiveUser(this IdentityUser user)
        {
            if (user == null) return false;
            return !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;
        }
    }
}
