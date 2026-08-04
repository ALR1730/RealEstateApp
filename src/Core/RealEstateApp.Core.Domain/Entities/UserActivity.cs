using System;

namespace RealEstateApp.Core.Domain.Entities
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-activity";
        public string? TargetUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
