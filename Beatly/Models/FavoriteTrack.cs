using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Beatly.Models
{
    public class FavoriteTrack
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrackId { get; set; }

        [ForeignKey("TrackId")]
        public Track Track { get; set; } = null!;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}