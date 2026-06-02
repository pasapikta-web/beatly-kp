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
    }
}