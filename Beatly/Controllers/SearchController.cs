using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Beatly.Data;

namespace Beatly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> GetSuggestions(string? query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 1)
            {
                return BadRequest();
            }

            var lowerQuery = query.ToLower();

            var tracks = await _context.Tracks
                .Where(t => t.Title != null && t.Artist != null &&
                            (t.Title.ToLower().Contains(lowerQuery) || t.Artist.ToLower().Contains(lowerQuery)))
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    Title = t.Title ?? "Без названия",
                    Artist = t.Artist ?? "Неизвестный исполнитель",
                    CoverPath = t.CoverPath ?? "/images/default-cover.png",
                    Url = t.Url ?? ""
                })
                .ToListAsync();

            var artists = await _context.Tracks
                .Where(t => t.Artist != null && t.Artist.ToLower().Contains(lowerQuery))
                .Select(t => t.Artist!)
                .Distinct()
                .Take(4)
                .ToListAsync();

            return Ok(new { tracks, artists });
        }
    }
}