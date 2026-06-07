using System;

namespace Beatly.Models
{
    public class FavoriteAlbum
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string AlbumName { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}