using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application.DTOs.Agent;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para consultar agentes y modificar su estado.
    /// </summary>
    public class AgentService : IAgentService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public AgentService(
            UserManager<IdentityUser> userManager,
            IPropertyRepository propertyRepository,
            IAccountService accountService,
            IMapper mapper)
        {
            _userManager = userManager;
            _propertyRepository = propertyRepository;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task<List<AgentViewModel>> GetAllViewModelAsync()
        {
            var agentUsers = await _userManager.GetUsersInRoleAsync(Roles.Agent.ToString());
            var allProperties = await _propertyRepository.GetAllAsync();

            var agentVms = new List<AgentViewModel>();

            foreach (var user in agentUsers)
            {
                var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();
                var isActive = !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;

                agentVms.Add(new AgentViewModel
                {
                    Id = user.Id,
                    FirstName = user.UserName ?? string.Empty,
                    LastName = string.Empty, // Se mapea desde nombre o claims
                    Email = user.Email ?? string.Empty,
                    Phone = user.PhoneNumber,
                    IsActive = isActive,
                    PropertiesCount = agentProperties.Count,
                    Properties = _mapper.Map<List<PropertyViewModel>>(agentProperties)
                });
            }

            return agentVms.OrderBy(a => a.FirstName).ToList();
        }

        public async Task<AgentViewModel?> GetByIdViewModelAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Agent.ToString())) return null;

            var allProperties = await _propertyRepository.GetAllAsync();
            var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();
            var isActive = !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;

            return new AgentViewModel
            {
                Id = user.Id,
                FirstName = user.UserName ?? string.Empty,
                LastName = string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                IsActive = isActive,
                PropertiesCount = agentProperties.Count,
                Properties = _mapper.Map<List<PropertyViewModel>>(agentProperties)
            };
        }

        public async Task<List<AgentDto>> GetAllDtoAsync()
        {
            var agentUsers = await _userManager.GetUsersInRoleAsync(Roles.Agent.ToString());
            var allProperties = await _propertyRepository.GetAllAsync();

            var agentDtos = new List<AgentDto>();

            foreach (var user in agentUsers)
            {
                var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();
                var isActive = !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;

                agentDtos.Add(new AgentDto
                {
                    Id = user.Id,
                    FirstName = user.UserName ?? string.Empty,
                    LastName = string.Empty,
                    Email = user.Email ?? string.Empty,
                    Phone = user.PhoneNumber,
                    IsActive = isActive,
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
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Agent.ToString())) return null;

            var allProperties = await _propertyRepository.GetAllAsync();
            var agentProperties = allProperties.Where(p => p.AgentId == user.Id).ToList();
            var isActive = !user.LockoutEnabled || !user.LockoutEnd.HasValue || user.LockoutEnd.Value <= DateTimeOffset.UtcNow;

            return new AgentDto
            {
                Id = user.Id,
                FirstName = user.UserName ?? string.Empty,
                LastName = string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                IsActive = isActive,
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
            var user = await _userManager.FindByIdAsync(agentId);
            if (user == null)
            {
                throw new Exception($"No existe un agente registrado con el ID: '{agentId}'");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Agent.ToString()))
            {
                throw new Exception($"El usuario con ID '{agentId}' no tiene el rol de Agente");
            }

            await _accountService.ChangeUserStatusAsync(agentId, isActive);
        }
    }
}
