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
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Enums;
using RealEstateApp.Core.Domain.Exceptions;

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
        private readonly IAgentVerificationRepository _agentVerificationRepository;
        private readonly IMapper _mapper;

        public AgentService(
            IPropertyRepository propertyRepository,
            IAccountService accountService,
            IOfferRepository offerRepository,
            IChatRepository chatRepository,
            IFavoriteRepository favoriteRepository,
            IPropertyImageRepository propertyImageRepository,
            IFileStorageService fileStorageService,
            IAgentVerificationRepository agentVerificationRepository,
            IMapper mapper)
        {
            _propertyRepository = propertyRepository;
            _accountService = accountService;
            _offerRepository = offerRepository;
            _chatRepository = chatRepository;
            _favoriteRepository = favoriteRepository;
            _propertyImageRepository = propertyImageRepository;
            _fileStorageService = fileStorageService;
            _agentVerificationRepository = agentVerificationRepository;
            _mapper = mapper;
        }

        public async Task<List<AgentViewModel>> GetAllViewModelAsync()
        {
            var agentUsers = await _accountService.GetUsersInRoleAsync(Roles.Agent.ToString());
            var allProperties = await _propertyRepository.GetAllAsync();
            var verifications = await _agentVerificationRepository.GetAllAsync();
            var verifiedAgentIds = verifications
                .Where(v => v.Status == VerificationStatus.Approved)
                .Select(v => v.AgentId)
                .ToHashSet();

            var agentVms = new List<AgentViewModel>();

            foreach (var user in agentUsers)
            {
                var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();

                agentVms.Add(new AgentViewModel
                {
                    Id = user.Id,
                    FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? user.UserName : user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    IsActive = user.IsActive,
                    IsVerified = verifiedAgentIds.Contains(user.Id),
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
            var verification = await _agentVerificationRepository.GetByAgentIdAsync(id);
            bool isVerified = verification != null && verification.Status == VerificationStatus.Approved;

            return new AgentViewModel
            {
                Id = user.Id,
                FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? user.UserName : user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsActive = user.IsActive,
                IsVerified = isVerified,
                PropertiesCount = agentProperties.Count,
                Properties = _mapper.Map<List<PropertyViewModel>>(agentProperties)
            };
        }

        public async Task<List<AgentDto>> GetAllDtoAsync()
        {
            var agentUsers = await _accountService.GetUsersInRoleAsync(Roles.Agent.ToString());
            var allProperties = await _propertyRepository.GetAllAsync();
            var verifications = await _agentVerificationRepository.GetAllAsync();
            var verifiedAgentIds = verifications
                .Where(v => v.Status == VerificationStatus.Approved)
                .Select(v => v.AgentId)
                .ToHashSet();

            var agentDtos = new List<AgentDto>();

            foreach (var user in agentUsers)
            {
                var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();

                agentDtos.Add(new AgentDto
                {
                    Id = user.Id,
                    FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? user.UserName : user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    IsActive = user.IsActive,
                    IsVerified = verifiedAgentIds.Contains(user.Id),
                    PropertiesCount = agentProperties.Count,
                    Properties = agentProperties.Select(p => new AgentPropertyDto
                    {
                        Id = p.Id,
                        Name = p.Name,
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
            var verification = await _agentVerificationRepository.GetByAgentIdAsync(id);
            bool isVerified = verification != null && verification.Status == VerificationStatus.Approved;

            return new AgentDto
            {
                Id = user.Id,
                FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? user.UserName : user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsActive = user.IsActive,
                IsVerified = isVerified,
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
                throw new NotFoundException($"No existe un agente registrado con el ID: '{agentId}'");
            }

            if (!user.Roles.Contains(Roles.Agent.ToString()))
            {
                throw new ValidationException($"El usuario con ID '{agentId}' no tiene el rol de Agente");
            }

            await _accountService.ChangeUserStatusAsync(agentId, isActive);
        }

        public async Task DeleteAgentCascadeAsync(string agentId)
        {
            var agent = await _accountService.GetUserByIdAsync(agentId);
            if (agent == null)
            {
                throw new NotFoundException($"El agente con ID '{agentId}' no existe");
            }

            // 1. Obtener todas las propiedades de este agente vía consulta directa en BD
            var agentProperties = await _propertyRepository.GetByAgentIdAsync(agentId);

            foreach (var prop in agentProperties)
            {
                // A. Borrar imágenes físicas de disco e imágenes en BD en lote
                var images = await _propertyImageRepository.GetByPropertyIdAsync(prop.Id);
                foreach (var img in images)
                {
                    await _fileStorageService.DeleteFileAsync(img.ImageUrl, "properties");
                }
                if (images.Any())
                {
                    await _propertyImageRepository.DeleteRangeAsync(images);
                }

                // B. Limpiar favoritos de la propiedad en lote vía SQL en BD
                var propFavs = await _favoriteRepository.GetByPropertyIdAsync(prop.Id);
                if (propFavs.Any())
                {
                    await _favoriteRepository.DeleteRangeAsync(propFavs);
                }

                // C. Limpiar ofertas de la propiedad en lote vía SQL en BD
                var offers = await _offerRepository.GetByPropertyIdAsync(prop.Id);
                if (offers.Any())
                {
                    await _offerRepository.DeleteRangeAsync(offers);
                }

                // D. Limpiar chats vinculados a la propiedad en lote vía SQL en BD
                var propChats = await _chatRepository.GetByPropertyIdAsync(prop.Id);
                if (propChats.Any())
                {
                    await _chatRepository.DeleteRangeAsync(propChats);
                }
            }

            // E. Eliminar las propiedades en lote
            if (agentProperties.Any())
            {
                await _propertyRepository.DeleteRangeAsync(agentProperties);
            }

            // 2. Limpiar cualquier chat del agente que no esté vinculado a propiedades específicas en lote
            var agentChats = await _chatRepository.GetByUserIdAsync(agentId);
            if (agentChats.Any())
            {
                await _chatRepository.DeleteRangeAsync(agentChats);
            }

            // 3. Eliminar usuario de Identity vía servicio de abstracción
            await _accountService.DeleteUserAsync(agentId);
        }
    }
}
