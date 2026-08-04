using System;

namespace RealEstateApp.Core.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "General"; // Offer, Chat, Account, General
        public bool IsRead { get; set; } = false;
        public string? RedirectUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
