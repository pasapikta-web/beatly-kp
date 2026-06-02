using System.Collections.Generic;

namespace Beatly.Models;

public class ArtistProfileViewModel
{
    public string ArtistName { get; set; } = string.Empty;
    public string AvatarLetter { get; set; } = string.Empty;
    public int FollowersCount { get; set; }
    public List<Track> Tracks { get; set; } = new();
}