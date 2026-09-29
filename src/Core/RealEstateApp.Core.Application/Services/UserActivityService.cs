using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.UserActivity;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly IUserActivityRepository _userActivityRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserActivityService> _logger;

        public UserActivityService(
            IUserActivityRepository userActivityRepository,
            IMapper mapper,
            ILogger<UserActivityService>? logger = null)
        {
            _userActivityRepository = userActivityRepository;
            _mapper = mapper;
            _logger = logger ?? NullLogger<UserActivityService>.Instance;
        }

        public async Task LogActivityAsync(string userId, string action, string description, string icon = "bi-activity", string? targetUrl = null)
        {
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                var activity = new UserActivity
                {
                    UserId = userId,
                    Action = action,
                    Description = description,
                    Icon = icon,
                    TargetUrl = targetUrl,
                    CreatedAt = DateTime.UtcNow
                };

                await _userActivityRepository.AddAsync(activity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al registrar actividad para el usuario {UserId} ({Action})", userId, action);
            }
        }

        public async Task<List<UserActivityViewModel>> GetRecentActivitiesAsync(string userId, int count = 20)
        {
            if (string.IsNullOrEmpty(userId)) return new List<UserActivityViewModel>();

            var activities = await _userActivityRepository.GetByUserIdAsync(userId, count);
            return _mapper.Map<List<UserActivityViewModel>>(activities);
        }
    }
}
