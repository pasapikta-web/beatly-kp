using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Beatly.Models
{
    public class Track
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название трека обязательно")]
        [StringLength(100, ErrorMessage = "Название слишком длинное")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя артиста обязательно")]
        public string Artist { get; set; } = string.Empty;

        public string Album { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите жанр")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите настроение")]
        public string Mood { get; set; } = string.Empty;

        public string AudioUrl { get; set; } = string.Empty;

        public string LosslessAudioUrl { get; set; } = string.Empty;

        public string CoverUrl { get; set; } = string.Empty;

        [NotMapped]
        public string CoverPath
        {
            get => CoverUrl;
            set => CoverUrl = value;
        }
        public string? Url { get; set; }

        [RegularExpression(@"^\d{1,2}:\d{2}$", ErrorMessage = "Формат длительности должен быть М:СС")]
        public string Duration { get; set; } = "0:00";

        public bool IsEarlyAccess { get; set; }

        [NotMapped]
        public bool IsLiked { get; set; }

        public List<FavoriteTrack> FavoriteTracks { get; set; } = new();
    }
}