using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Favorite;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de favoritos con depuración automática de propiedades vendidas.
    /// El repositorio ya filtra las propiedades vendidas al consultar.
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IMapper _mapper;

        public FavoriteService(IFavoriteRepository favoriteRepository, IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _mapper = mapper;
        }

        public async Task<List<FavoriteViewModel>> GetByClienteId(string clienteId)
        {
            var favorites = await _favoriteRepository.GetByClienteIdAsync(clienteId);
            return _mapper.Map<List<FavoriteViewModel>>(favorites);
        }

        public async Task AddFavorite(string clienteId, int propertyId)
        {
            // Verificar que no exista ya
            var existing = await _favoriteRepository.GetByClienteAndPropertyAsync(clienteId, propertyId);
            if (existing != null) return; // Ya es favorito, no duplicar

            var favorite = new Favorite
            {
                ClienteId = clienteId,
                PropertyId = propertyId
            };

            await _favoriteRepository.AddAsync(favorite);
        }

        public async Task RemoveFavorite(string clienteId, int propertyId)
        {
            var favorite = await _favoriteRepository.GetByClienteAndPropertyAsync(clienteId, propertyId);
            if (favorite == null) return;

            await _favoriteRepository.DeleteAsync(favorite);
        }

        public async Task<bool> IsFavorite(string clienteId, int propertyId)
        {
            var favorite = await _favoriteRepository.GetByClienteAndPropertyAsync(clienteId, propertyId);
            return favorite != null;
        }
    }
}
