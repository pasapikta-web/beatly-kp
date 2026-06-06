using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Beatly.Controllers
{
    [Authorize]
    public class TrackController : Controller
    {
        [HttpGet]
        public IActionResult Download(string fileUrl, string title)
        {
            if (!User.HasClaim(c => c.Type == "Premium" && c.Value == "True"))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(fileUrl))
            {
                return BadRequest();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileUrl.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = $"{title}.mp3";

            return File(fileBytes, "audio/mpeg", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadMultiple([FromServices] Beatly.Data.ApplicationDbContext _context, [FromServices] Microsoft.AspNetCore.Identity.UserManager<Beatly.Models.User> _userManager, [FromBody] int[] trackIds)
        {
            if (!User.HasClaim(c => c.Type == "Premium" && c.Value == "True"))
            {
                return Forbid();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            IQueryable<Beatly.Models.Track> tracksQuery;

            if (trackIds == null || trackIds.Length == 0)
            {
                tracksQuery = _context.FavoriteTracks
                    .Where(ft => ft.UserId == user.Id)
                    .Select(ft => ft.Track);
            }
            else
            {
                tracksQuery = _context.Tracks.Where(t => trackIds.Contains(t.Id));
            }

            var tracks = tracksQuery.ToList();

            if (!tracks.Any())
            {
                return BadRequest("No tracks selected.");
            }

            using var memoryStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var track in tracks)
                {
                    if (string.IsNullOrEmpty(track.AudioUrl)) continue;
                    
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", track.AudioUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        var entryName = $"{track.Artist} - {track.Title}.mp3";
                        entryName = string.Join("_", entryName.Split(Path.GetInvalidFileNameChars()));
                        archive.CreateEntryFromFile(filePath, entryName);
                    }
                }
            }

            return File(memoryStream.ToArray(), "application/zip", "Beatly_Tracks.zip");
        }
    }
}