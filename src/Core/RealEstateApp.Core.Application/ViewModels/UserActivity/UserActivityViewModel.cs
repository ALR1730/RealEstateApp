using System;

namespace RealEstateApp.Core.Application.ViewModels.UserActivity
{
    public class UserActivityViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-activity";
        public string? TargetUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string TimeAgo
        {
            get
            {
                var span = DateTime.UtcNow - CreatedAt;
                if (span.TotalMinutes < 1) return "Hace un momento";
                if (span.TotalMinutes < 60) return $"Hace {(int)span.TotalMinutes} min";
                if (span.TotalHours < 24) return $"Hace {(int)span.TotalHours} hs";
                if (span.TotalDays < 7) return $"Hace {(int)span.TotalDays} días";
                return CreatedAt.ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}
