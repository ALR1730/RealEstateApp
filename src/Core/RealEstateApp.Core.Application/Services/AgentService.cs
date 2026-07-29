using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using RealEstateApp.Core.Application.DTOs.Agent;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para consultar agentes, modificar su estado y eliminación física en cascada.
    /// Desacoplado 100% de la infraestructura de Identity cumpliendo con la Arquitectura Onion.
    /// </summary>
    public class AgentService : IAgentService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAccountService _accountService;
        private readonly IOfferRepository _offerRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IPropertyImageRepository _propertyImageRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;

        public AgentService(
            IPropertyRepository propertyRepository,
            IAccountService accountService,
            IOfferRepository offerRepository,
            IChatRepository chatRepository,
            IFavoriteRepository favoriteRepository,
            IPropertyImageRepository propertyImageRepository,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _accountService = accountService;
            _offerRepository = offerRepository;
            _chatRepository = chatRepository;
            _favoriteRepository = favoriteRepository;
            _propertyImageRepository = propertyImageRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<List<AgentViewModel>> GetAllViewModelAsync()
        {
            var agentUsers = await _accountService.GetUsersInRoleAsync(Roles.Agent.ToString());
            var allProperties = await _propertyRepository.GetAllAsync();

            var agentVms = new List<AgentViewModel>();

            foreach (var user in agentUsers)
            {
                var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();

                agentVms.Add(new AgentViewModel
                {
                    Id = user.Id,
                    FirstName = user.UserName,
                    LastName = string.Empty,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    IsActive = user.IsActive,
                    PropertiesCount = agentProperties.Count,
                    Properties = _mapper.Map<List<PropertyViewModel>>(agentProperties)
                });
            }

            return agentVms.OrderBy(a => a.FirstName).ToList();
        }

        public async Task<AgentViewModel?> GetByIdViewModelAsync(string id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            if (user == null || !user.Roles.Contains(Roles.Agent.ToString())) return null;

            var allProperties = await _propertyRepository.GetAllAsync();
            var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();

            return new AgentViewModel
            {
                Id = user.Id,
                FirstName = user.UserName,
                LastName = string.Empty,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsActive = user.IsActive,
                PropertiesCount = agentProperties.Count,
                Properties = _mapper.Map<List<PropertyViewModel>>(agentProperties)
            };
        }

        public async Task<List<AgentDto>> GetAllDtoAsync()
        {
            var agentUsers = await _accountService.GetUsersInRoleAsync(Roles.Agent.ToString());
            var allProperties = await _propertyRepository.GetAllAsync();

            var agentDtos = new List<AgentDto>();

            foreach (var user in agentUsers)
            {
                var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();

                agentDtos.Add(new AgentDto
                {
                    Id = user.Id,
                    FirstName = user.UserName,
                    LastName = string.Empty,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    IsActive = user.IsActive,
                    PropertiesCount = agentProperties.Count,
                    Properties = agentProperties.Select(p => new AgentPropertyDto
                    {
                        Id = p.Id,
                        Code = p.Code,
                        Price = p.Price,
                        Rooms = p.Rooms,
                        Bathrooms = p.Bathrooms,
                        SizeInMeters = p.SizeInMeters,
                        Status = p.Status
                    }).ToList()
                });
            }

            return agentDtos.OrderBy(a => a.FirstName).ToList();
        }

        public async Task<AgentDto?> GetByIdDtoAsync(string id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            if (user == null || !user.Roles.Contains(Roles.Agent.ToString())) return null;

            var allProperties = await _propertyRepository.GetAllAsync();
            var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();

            return new AgentDto
            {
                Id = user.Id,
                FirstName = user.UserName,
                LastName = string.Empty,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsActive = user.IsActive,
                PropertiesCount = agentProperties.Count,
                Properties = agentProperties.Select(p => new AgentPropertyDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Price = p.Price,
                    Rooms = p.Rooms,
                    Bathrooms = p.Bathrooms,
                    SizeInMeters = p.SizeInMeters,
                    Status = p.Status
                }).ToList()
            };
        }

        public async Task ChangeStatusAsync(string agentId, bool isActive)
        {
            var user = await _accountService.GetUserByIdAsync(agentId);
            if (user == null)
            {
                throw new Exception($"No existe un agente registrado con el ID: '{agentId}'");
            }

            if (!user.Roles.Contains(Roles.Agent.ToString()))
            {
                throw new Exception($"El usuario con ID '{agentId}' no tiene el rol de Agente");
            }

            await _accountService.ChangeUserStatusAsync(agentId, isActive);
        }

        public async Task DeleteAgentCascadeAsync(string agentId)
        {
            var agent = await _accountService.GetUserByIdAsync(agentId);
            if (agent == null)
            {
                throw new Exception($"El agente con ID '{agentId}' no existe");
            }

            // 1. Obtener todas las propiedades de este agente
            var properties = await _propertyRepository.GetAllAsync();
            var agentProperties = properties.Where(p => p.AgentId == agentId).ToList();

            foreach (var prop in agentProperties)
            {
                // A. Borrar imágenes físicas de disco e imágenes en BD
                var images = await _propertyImageRepository.GetByPropertyIdAsync(prop.Id);
                foreach (var img in images)
                {
                    await _fileStorageService.DeleteFileAsync(img.ImageUrl, "properties");
                    await _propertyImageRepository.DeleteAsync(img);
                }

                // B. Limpiar favoritos de la propiedad
                var favorites = await _favoriteRepository.GetAllAsync();
                var propFavs = favorites.Where(f => f.PropertyId == prop.Id).ToList();
                foreach (var fav in propFavs)
                {
                    await _favoriteRepository.DeleteAsync(fav);
                }

                // C. Limpiar ofertas de la propiedad
                var offers = await _offerRepository.GetByPropertyIdAsync(prop.Id);
                foreach (var offer in offers)
                {
                    await _offerRepository.DeleteAsync(offer);
                }

                // D. Limpiar chats vinculados a la propiedad
                var chats = await _chatRepository.GetAllAsync();
                var propChats = chats.Where(c => c.PropertyId == prop.Id).ToList();
                foreach (var chat in propChats)
                {
                    await _chatRepository.DeleteAsync(chat);
                }

                // E. Eliminar la propiedad
                await _propertyRepository.DeleteAsync(prop);
            }

            // 2. Limpiar cualquier chat del agente que no esté vinculado a propiedades específicas
            var remainingChats = await _chatRepository.GetAllAsync();
            var agentChats = remainingChats.Where(c => c.ClienteId == agentId || c.AgenteId == agentId || c.SenderId == agentId).ToList();
            foreach (var chat in agentChats)
            {
                await _chatRepository.DeleteAsync(chat);
            }

            // 3. Eliminar usuario de Identity vía servicio de abstracción
            await _accountService.DeleteUserAsync(agentId);
        }
    }
}
