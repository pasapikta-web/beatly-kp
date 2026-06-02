using System.Collections.Generic;

namespace Beatly.Models
{
    public class DeviceSession
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string LastActive { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
    }

    public class ProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePicturePath { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public string SubscriptionPlan { get; set; }
        public string StreamingQuality { get; set; } = string.Empty;
        public bool IsEqualizerEnabled { get; set; }
        public string AppTheme { get; set; } = string.Empty;
        public string InstagramUrl { get; set; } = string.Empty;
        public string TelegramUrl { get; set; } = string.Empty;
        public List<DeviceSession> Devices { get; set; } = new List<DeviceSession>();
    }
}