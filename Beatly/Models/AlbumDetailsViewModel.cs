using System.Collections.Generic;

namespace Beatly.Models
{
    public class AlbumDetailsViewModel
    {
        public string Name { get; set; } = string.Empty;
        public List<Track> Tracks { get; set; } = new List<Track>();
    }
}