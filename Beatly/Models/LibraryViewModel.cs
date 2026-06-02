using System.Collections.Generic;
using System.Linq;

namespace Beatly.Models;

public class LibraryViewModel
{
    public List<Track> AllLikedTracks { get; set; } = new();
    public List<IGrouping<string?, Track>> Albums { get; set; } = new();
    public List<IGrouping<string?, Track>> Artists { get; set; } = new();
}