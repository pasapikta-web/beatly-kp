using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Beatly.Data;
using Beatly.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beatly.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null) return RedirectToAction("Login", "Account");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            var model = new ProfileViewModel
            {
                FullName = identityUser.UserName ?? string.Empty,
                Email = identityUser.Email ?? string.Empty,
                ProfilePicturePath = "/api/placeholder/128/128",
                IsPremium = User.IsInRole("Premium"),
                StreamingQuality = Request.Cookies["Quality"] ?? "High",
                IsEqualizerEnabled = Request.Cookies["EqEnabled"] == "true",
                AppTheme = Request.Cookies["AppTheme"] ?? "Dark",
                InstagramUrl = dbUser?.InstagramUrl ?? string.Empty,
                TelegramUrl = dbUser?.TelegramUrl ?? string.Empty,
                Devices = new List<DeviceSession>
                {
                    new DeviceSession { Name = "Windows PC - Chrome", Location = "Текущая сессия", LastActive = "Сейчас", IsCurrent = true }
                }
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveSettings(string quality, bool eqEnabled, string theme)
        {
            Response.Cookies.Append("Quality", quality ?? "High");
            Response.Cookies.Append("EqEnabled", eqEnabled.ToString().ToLower());
            Response.Cookies.Append("AppTheme", theme ?? "Dark");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] UpdatePasswordModel model)
        {
            if (model == null) return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return BadRequest();

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword ?? string.Empty, model.NewPassword ?? string.Empty);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                return Ok();
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> SocialMedia()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null) return NotFound();

            var model = new SocialMediaViewModel
            {
                InstagramUrl = user.InstagramUrl ?? string.Empty,
                TelegramUrl = user.TelegramUrl ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SocialMedia(SocialMediaViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null) return NotFound();

            user.InstagramUrl = model.InstagramUrl ?? string.Empty;
            user.TelegramUrl = model.TelegramUrl ?? string.Empty;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}