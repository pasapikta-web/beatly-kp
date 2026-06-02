using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Beatly.Models;

namespace Beatly.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Track> Tracks { get; set; } = null!;
        public DbSet<FavoriteTrack> FavoriteTracks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FavoriteTrack>()
                .HasKey(ft => new { ft.UserId, ft.TrackId });

            builder.Entity<FavoriteTrack>()
                .HasOne(ft => ft.User)
                .WithMany()
                .HasForeignKey(ft => ft.UserId);

            builder.Entity<FavoriteTrack>()
                .HasOne(ft => ft.Track)
                .WithMany(t => t.FavoriteTracks)
                .HasForeignKey(ft => ft.TrackId);
        }
    }
}