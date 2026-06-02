using Microsoft.AspNetCore.Identity;

namespace Beatly.Models
{

    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public string? SubscriptionPlan { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TelegramUrl { get; set; }
    }
}