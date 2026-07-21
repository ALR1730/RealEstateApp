using System.Collections.Generic;
using System.Threading.Tasks;
using RealEstateApp.Core.Application.ViewModels.Favorite;

namespace RealEstateApp.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Servicio de aplicación para gestión de favoritos del cliente.
    /// Incluye depuración automática de propiedades vendidas.
    /// </summary>
    public interface IFavoriteService
    {
        /// <summary>
        /// Obtiene los favoritos del cliente (excluyendo propiedades vendidas).
        /// </summary>
        Task<List<FavoriteViewModel>> GetByClienteId(string clienteId);

        /// <summary>
        /// Agrega una propiedad a favoritos. Si ya existe, no crea duplicado.
        /// </summary>
        Task AddFavorite(string clienteId, int propertyId);

        /// <summary>
        /// Elimina una propiedad de favoritos.
        /// </summary>
        Task RemoveFavorite(string clienteId, int propertyId);

        /// <summary>
        /// Verifica si una propiedad está en los favoritos del cliente.
        /// </summary>
        Task<bool> IsFavorite(string clienteId, int propertyId);
    }
}
