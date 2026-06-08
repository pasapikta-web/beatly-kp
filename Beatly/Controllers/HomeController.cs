using Microsoft.AspNetCore.Mvc;
using Beatly.Data;
using Beatly.Models;
using Beatly.Services;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beatly.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly NotificationService _notificationService;

        public HomeController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IWebHostEnvironment webHostEnvironment,
            NotificationService notificationService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        private async Task SetLikedStatusAsync(IEnumerable<Track> tracks)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var likedTrackIds = await _context.FavoriteTracks
                .Where(ft => ft.UserId == user.Id)
                .Select(ft => ft.TrackId)
                .ToListAsync();

            foreach (var track in tracks)
            {
                track.IsLiked = likedTrackIds.Contains(track.Id);
            }
        }

        private void SetNotificationCount()
        {
            string username = User?.Identity?.Name ?? string.Empty;
            ViewBag.UnreadNotifications = string.IsNullOrEmpty(username) ? 0 : _notificationService.GetUnreadCount(username);
        }

        [HttpGet]
        public async Task<IActionResult> GetPlaybackQueue(string contextType, string contextValue)
        {
            IQueryable<Track> query = _context.Tracks;
            switch (contextType)
            {
                case "Album":
                    query = query.Where(t => t.Album == contextValue);
                    break;
                case "Artist":
                    query = query.Where(t => t.Artist == contextValue);
                    break;
                case "Genre":
                    query = query.Where(t => t.Genre == contextValue);
                    break;
                case "Mood":
                    query = query.Where(t => t.Mood == contextValue);
                    break;
                case "Favorites":
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null) return Json(new { tracks = new List<int>() });
                    query = _context.FavoriteTracks.Where(ft => ft.UserId == user.Id).Select(ft => ft.Track);
                    break;
            }

            var trackIds = await query.OrderBy(t => t.Id).Select(t => t.Id).ToListAsync();
            return Json(new { tracks = trackIds });
        }

        public async Task<IActionResult> Index()
        {
            SetNotificationCount();
            var tracks = await _context.Tracks.ToListAsync();
            await SetLikedStatusAsync(tracks);

            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(tracks, jsonOptions);

            return View(tracks ?? new List<Track>());
        }

        public async Task<IActionResult> Discovery()
        {
            SetNotificationCount();
            var tracks = await _context.Tracks.ToListAsync();
            await SetLikedStatusAsync(tracks);

            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(tracks, jsonOptions);

            List<IGrouping<string?, Track>> groupedTracks = tracks
                .GroupBy(t => (string?)t.Mood)
                .ToList();
            return View(groupedTracks);
        }

        public IActionResult Browser()
        {
            return RedirectToAction("Discovery");
        }

        public async Task<IActionResult> Search(string query)
        {
            SetNotificationCount();
            var tracksQuery = _context.Tracks.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                tracksQuery = tracksQuery.Where(t =>
                    t.Title.Contains(query) || t.Artist.Contains(query) || t.Album.Contains(query));
            }

            var tracks = await tracksQuery.ToListAsync();
            await SetLikedStatusAsync(tracks);
            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(tracks, jsonOptions);

            return View(tracks ?? new List<Track>());
        }

        public async Task<IActionResult> Category(string type, string name)
        {
            SetNotificationCount();
            var tracksQuery = _context.Tracks.AsQueryable();

            if (string.Equals(type, "Genre", StringComparison.OrdinalIgnoreCase))
            {
                tracksQuery = tracksQuery.Where(t => t.Genre == name);
            }
            else if (string.Equals(type, "Mood", StringComparison.OrdinalIgnoreCase))
            {
                tracksQuery = tracksQuery.Where(t => t.Mood == name);
            }
            else if (string.Equals(type, "Album", StringComparison.OrdinalIgnoreCase))
            {
                tracksQuery = tracksQuery.Where(t => t.Album == name);
            }

            var tracks = await tracksQuery.ToListAsync();
            await SetLikedStatusAsync(tracks);
            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(tracks, jsonOptions);
            var viewModel = new CategoryViewModel
            {
                Name = name,
                Tracks = tracks
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Library()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            SetNotificationCount();

            var likedTracks = await _context.FavoriteTracks
                .Where(ft => ft.UserId == user.Id)
                .Include(ft => ft.Track)
                .Select(ft => ft.Track)
                .ToListAsync();
            foreach (var t in likedTracks) t.IsLiked = true;

            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(likedTracks, jsonOptions);

            var viewModel = new LibraryViewModel
            {
                AllLikedTracks = likedTracks,
                Albums = likedTracks.Where(t => !string.IsNullOrEmpty(t.Album)).GroupBy(t => (string?)t.Album).ToList(),
                Artists = likedTracks.Where(t => !string.IsNullOrEmpty(t.Artist)).GroupBy(t => (string?)t.Artist).ToList()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Favorites()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            SetNotificationCount();

            var likedTracks = await _context.FavoriteTracks
                .Where(ft => ft.UserId == user.Id)
                .Include(ft => ft.Track)
                .Select(ft => ft.Track)
                .ToListAsync();
            foreach (var t in likedTracks) t.IsLiked = true;

            var followedArtists = await _context.FollowedArtists
                .Where(fa => fa.UserId == user.Id)
                .Select(fa => new FollowedArtistViewModel
                {
                    ArtistName = fa.ArtistName,
                    AvatarLetter = string.IsNullOrEmpty(fa.ArtistName) ? "?" : fa.ArtistName.Substring(0, 1).ToUpper()
                })
                .ToListAsync();
            var dbFavoriteAlbums = await _context.FavoriteAlbums
                .Where(fa => fa.UserId == user.Id)
                .ToListAsync();
            foreach (var album in dbFavoriteAlbums)
            {
                var firstTrack = await _context.Tracks
                    .FirstOrDefaultAsync(t => t.Album == album.AlbumName && !string.IsNullOrEmpty(t.CoverUrl));
                if (firstTrack != null)
                {
                    var cleanCover = firstTrack.CoverUrl.Replace("wwwroot/", "").Replace("wwwroot\\", "").Replace("\\", "/");
                    if (!cleanCover.StartsWith("/") && !cleanCover.StartsWith("http"))
                    {
                        cleanCover = "/" + cleanCover;
                    }
                    album.CoverUrl = cleanCover;
                }
                else
                {
                    album.CoverUrl = "https://placehold.co/200x200?text=No+Cover";
                }
            }

            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(likedTracks, jsonOptions);

            var viewModel = new FavoritesViewModel
            {
                LikedTracks = likedTracks,
                FollowedArtists = followedArtists,
                LikedAlbums = dbFavoriteAlbums
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Авторизуйтесь" });

            var track = await _context.Tracks.FindAsync(id);
            if (track == null) return NotFound();

            var favorite = await _context.FavoriteTracks
                .FirstOrDefaultAsync(ft => ft.UserId == user.Id && ft.TrackId == id);
            bool isLiked;
            if (favorite != null)
            {
                _context.FavoriteTracks.Remove(favorite);
                isLiked = false;
            }
            else
            {
                _context.FavoriteTracks.Add(new FavoriteTrack { UserId = user.Id, TrackId = id });
                isLiked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isLiked = isLiked });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFollowArtist(string artistName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Авторизуйтесь для подписки" });
            if (string.IsNullOrEmpty(artistName)) return Json(new { success = false, message = "Имя исполнителя не указано" });
            var existingFollow = await _context.FollowedArtists
                .FirstOrDefaultAsync(fa => fa.UserId == user.Id && fa.ArtistName == artistName);
            bool isFollowing;
            if (existingFollow != null)
            {
                _context.FollowedArtists.Remove(existingFollow);
                isFollowing = false;
            }
            else
            {
                var newFollow = new FollowedArtist
                {
                    UserId = user.Id,
                    ArtistName = artistName
                };
                _context.FollowedArtists.Add(newFollow);
                isFollowing = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isFollowing = isFollowing, message = isFollowing ? $"Вы подписались на {artistName}" : $"Вы отписались от {artistName}" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavoriteAlbum(string albumName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Авторизуйтесь для добавления в любимое" });
            if (string.IsNullOrEmpty(albumName)) return Json(new { success = false, message = "Название альбома не указано" });
            var existingAlbum = await _context.FavoriteAlbums
                .FirstOrDefaultAsync(fa => fa.UserId == user.Id && fa.AlbumName == albumName);
            bool isLiked;
            if (existingAlbum != null)
            {
                _context.FavoriteAlbums.Remove(existingAlbum);
                isLiked = false;
            }
            else
            {
                var newAlbum = new FavoriteAlbum
                {
                    UserId = user.Id,
                    AlbumName = albumName
                };
                _context.FavoriteAlbums.Add(newAlbum);
                isLiked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, isLiked = isLiked, message = isLiked ? $"Альбом '{albumName}' добавлен в любимое" : $"Альбом '{albumName}' удален из любимого" });
        }

        [HttpPost]
        public async Task<IActionResult> DownloadArtistTracks(string name)
        {
            if (!User.HasClaim(c => c.Type == "Premium" && c.Value == "True") && !User.IsInRole("Premium"))
            {
                return Forbid();
            }

            var tracks = await _context.Tracks.Where(t => t.Artist == name).ToListAsync();
            if (!tracks.Any()) return NotFound();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var track in tracks)
                    {
                        if (string.IsNullOrEmpty(track.AudioUrl)) continue;
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, track.AudioUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            var entry = archive.CreateEntry($"{track.Artist} - {track.Title}.mp3");
                            using (var entryStream = entry.Open())
                            using (var fileStream = System.IO.File.OpenRead(filePath))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                }
                return File(memoryStream.ToArray(), "application/zip", $"{name} - All Tracks.zip");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DownloadAlbumTracks(string name)
        {
            if (!User.HasClaim(c => c.Type == "Premium" && c.Value == "True") && !User.IsInRole("Premium"))
            {
                return Forbid();
            }

            var tracks = await _context.Tracks.Where(t => t.Album == name).ToListAsync();
            if (!tracks.Any()) return NotFound();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var track in tracks)
                    {
                        if (string.IsNullOrEmpty(track.AudioUrl)) continue;
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, track.AudioUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            var entry = archive.CreateEntry($"{track.Artist} - {track.Title}.mp3");
                            using (var entryStream = entry.Open())
                            using (var fileStream = System.IO.File.OpenRead(filePath))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                }
                return File(memoryStream.ToArray(), "application/zip", $"Album - {name}.zip");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DownloadMultiple([FromBody] List<int> trackIds)
        {
            if (trackIds == null || !trackIds.Any()) return BadRequest();
            var tracks = await _context.Tracks.Where(t => trackIds.Contains(t.Id)).ToListAsync();
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var track in tracks)
                    {
                        if (string.IsNullOrEmpty(track.AudioUrl)) continue;
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, track.AudioUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            var entry = archive.CreateEntry($"{track.Artist} - {track.Title}.mp3");
                            using (var entryStream = entry.Open())
                            using (var fileStream = System.IO.File.OpenRead(filePath))
                            {
                                await fileStream.CopyToAsync(entryStream);
                            }
                        }
                    }
                }
                return File(memoryStream.ToArray(), "application/zip", "Beatly_Tracks.zip");
            }
        }

        public async Task<IActionResult> DownloadTrack(int id)
        {
            if (!User.HasClaim(c => c.Type == "Premium" && c.Value == "True") && !User.IsInRole("Premium"))
            {
                return Forbid();
            }

            var track = await _context.Tracks.FindAsync(id);
            if (track == null || string.IsNullOrEmpty(track.AudioUrl))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, track.AudioUrl.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "audio/mpeg", $"{track.Artist} - {track.Title}.mp3");
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Track track, IFormFile trackFile, IFormFile coverFile)
        {
            ModelState.Remove("AudioUrl");
            ModelState.Remove("CoverUrl");

            if (ModelState.IsValid)
            {
                if (trackFile != null) track.AudioUrl = await SaveFile(trackFile, "tracks");
                if (coverFile != null) track.CoverUrl = await SaveFile(coverFile, "covers");

                _context.Add(track);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(track);
        }

        public IActionResult Notifications()
        {
            string username = User.Identity?.Name ?? string.Empty;
            ViewBag.UnreadNotifications = _notificationService.GetUnreadCount(username);
            return View(_notificationService.GetUserNotifications(username));
        }

        [HttpPost]
        public IActionResult MarkNotificationAsRead(int id)
        {
            var success = _notificationService.MarkAsRead(id, User.Identity?.Name ?? string.Empty);
            return Json(new { success });
        }

        public async Task<IActionResult> Albums()
        {
            var tracks = await _context.Tracks.ToListAsync();
            return View(tracks.Where(t => !string.IsNullOrEmpty(t.Album)).GroupBy(t => t.Album).ToList());
        }

        public async Task<IActionResult> AlbumDetails(string name)
        {
            if (string.IsNullOrEmpty(name)) return NotFound();
            SetNotificationCount();

            var tracks = await _context.Tracks.Where(t => t.Album == name).ToListAsync();
            await SetLikedStatusAsync(tracks);
            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(tracks, jsonOptions);

            var user = await _userManager.GetUserAsync(User);
            bool isFavorite = false;
            if (user != null)
            {
                isFavorite = await _context.FavoriteAlbums
                    .AnyAsync(fa => fa.UserId == user.Id && fa.AlbumName == name);
            }
            ViewBag.IsFavoriteAlbum = isFavorite;
            var viewModel = new Beatly.Models.AlbumDetailsViewModel
            {
                Name = name,
                Tracks = tracks
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Artists()
        {
            SetNotificationCount();
            var tracks = await _context.Tracks.ToListAsync();
            var grouped = tracks
                .Where(t => !string.IsNullOrEmpty(t.Artist))
                .GroupBy(t => t.Artist!)
                .ToList();
            return View(grouped);
        }

        public async Task<IActionResult> ArtistDetails(string name)
        {
            if (string.IsNullOrEmpty(name)) return NotFound();

            var artistTracks = await _context.Tracks
                .Where(t => t.Artist == name)
                .ToListAsync();

            SetNotificationCount();
            await SetLikedStatusAsync(artistTracks);
            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles };
            ViewBag.Playlist = JsonSerializer.Serialize(artistTracks, jsonOptions);

            ViewBag.ArtistName = name;
            var user = await _userManager.GetUserAsync(User);
            bool isFollowing = false;
            List<string> likedAlbumNames = new List<string>();
            if (user != null)
            {
                isFollowing = await _context.FollowedArtists
                    .AnyAsync(fa => fa.UserId == user.Id && fa.ArtistName == name);
                likedAlbumNames = await _context.FavoriteAlbums
                    .Where(fa => fa.UserId == user.Id)
                    .Select(fa => fa.AlbumName)
                    .ToListAsync();
            }
            ViewBag.IsFollowing = isFollowing;
            ViewBag.LikedAlbumNames = likedAlbumNames;
            var albumsData = artistTracks
                .Where(t => !string.IsNullOrEmpty(t.Album))
                .GroupBy(t => t.Album)
                .Select(g => new
                {
                    AlbumName = g.Key,
                    CoverUrl = g.FirstOrDefault(t => !string.IsNullOrEmpty(t.CoverUrl) && !t.CoverUrl.Contains("default"))?.CoverUrl
                               ?? "https://placehold.co/200x200?text=No+Cover"
                })
                .ToList()
                .Select(a => {
                    var cleanCover = a.CoverUrl.Replace("wwwroot/", "").Replace("wwwroot\\", "").Replace("\\", "/");
                    if (!cleanCover.StartsWith("/") && !cleanCover.StartsWith("http"))
                    {
                        cleanCover = "/" + cleanCover;
                    }
                    return new Tuple<string, string>(a.AlbumName!, cleanCover);
                })
                .ToList();
            ViewBag.Albums = albumsData;

            return View(artistTracks);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            SetNotificationCount();

            bool isPremium = User.HasClaim(c => c.Type == "Premium" && c.Value == "True") || User.IsInRole("Premium");

            var model = new ProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePicturePath = user.ProfilePicturePath ?? "/api/placeholder/128/128",
                IsPremium = isPremium,
                SubscriptionPlan = user.SubscriptionPlan ?? string.Empty,
                StreamingQuality = Request.Cookies["Quality"] ?? "High",
                IsEqualizerEnabled = Request.Cookies["EqEnabled"] == "true",
                AppTheme = Request.Cookies["AppTheme"] ?? "Dark",
                Devices = new List<DeviceSession>()
            };
            ViewBag.IsPremium = isPremium;
            ViewBag.SubscriptionPlan = user.SubscriptionPlan;
            ViewBag.SubscriptionEndDate = user.SubscriptionEndDate;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, IFormFile? profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = fullName ?? string.Empty;
            user.Email = email;
            user.UserName = email;
            if (profilePicture != null && profilePicture.Length > 0)
            {
                user.ProfilePicturePath = await SaveFile(profilePicture, "profiles");
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded) return RedirectToAction("Profile");

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            var model = new ProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                ProfilePicturePath = user.ProfilePicturePath ?? "/api/placeholder/128/128",
                IsPremium = User.IsInRole("Premium"),
                StreamingQuality = Request.Cookies["Quality"] ?? "High",
                IsEqualizerEnabled = Request.Cookies["EqEnabled"] == "true",
                AppTheme = Request.Cookies["AppTheme"] ?? "Dark",
                Devices = new List<DeviceSession>()
            };
            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSocialLinks(string instagramUrl, string telegramUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.InstagramUrl = instagramUrl;
                user.TelegramUrl = telegramUrl;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Profile");
        }

        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Home");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            properties.Items["prompt"] = "select_account";
            return Challenge(properties, "Google");
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction("Login", "Account");

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded) return RedirectToAction("Index", "Home");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User { UserName = email, Email = email };
                    await _userManager.CreateAsync(user);
                }
                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Premium()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            SetNotificationCount();

            var model = new SubscriptionViewModel
            {
                PlanName = user.SubscriptionPlan ?? string.Empty,
                IsActive = user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value > DateTime.UtcNow,
                EndDate = user.SubscriptionEndDate
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string planName, decimal price, string cardNumber, string expiry, string cvv)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Авторизуйтесь" });

            user.SubscriptionPlan = planName;
            user.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);
            var claims = await _userManager.GetClaimsAsync(user);
            if (!claims.Any(c => c.Type == "Premium"))
            {
                await _userManager.AddClaimAsync(user, new Claim("Premium", "True"));
            }

            await _context.SaveChangesAsync();
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            _notificationService.AddNotification(
                user.UserName ?? string.Empty,
                "Подписка активирована!",
                $"Вы успешно оформили тариф '{planName}'.",
                "workspace_premium",
                "text-amber-400"
            );
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Premium");

            user.SubscriptionPlan = string.Empty;
            user.SubscriptionEndDate = null;

            var claims = await _userManager.GetClaimsAsync(user);
            var premiumClaim = claims.FirstOrDefault(c => c.Type == "Premium");
            if (premiumClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, premiumClaim);
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Premium");
        }

        private async Task<string> SaveFile(IFormFile file, string folderName)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", folderName);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            using (var stream = new FileStream(Path.Combine(uploadsFolder, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "/uploads/" + folderName + "/" + fileName;
        }
    }
}