using System;

namespace Beatly.Models
{
    public class FollowedArtist
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}