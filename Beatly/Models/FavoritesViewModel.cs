using System.Collections.Generic;

namespace Beatly.Models
{
    public class FavoritesViewModel
    {
        public IEnumerable<Track> LikedTracks { get; set; } = new List<Track>();
        public IEnumerable<FollowedArtistViewModel> FollowedArtists { get; set; } = new List<FollowedArtistViewModel>();
        public IEnumerable<FavoriteAlbum> LikedAlbums { get; set; } = new List<FavoriteAlbum>();
    }

    public class FollowedArtistViewModel
    {
        public string ArtistName { get; set; } = string.Empty;
        public int TracksCount { get; set; }
        public int AlbumsCount { get; set; }
        public string AvatarLetter { get; set; } = string.Empty;
    }
}