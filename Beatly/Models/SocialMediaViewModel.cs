using System.ComponentModel.DataAnnotations;

namespace Beatly.Models
{
    public class SocialMediaViewModel
    {
        [Required(ErrorMessage = "Укажите ссылку на Instagram")]
        [Url(ErrorMessage = "Некорректный формат URL")]
        [RegularExpression(@"^https?:\/\/(www\.)?instagram\.com\/[a-zA-Z0-9_\.]+\/?$", ErrorMessage = "Ссылка должна быть в формате https://instagram.com/username")]
        public string InstagramUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите ссылку на Telegram")]
        [Url(ErrorMessage = "Некорректный формат URL")]
        [RegularExpression(@"^https?:\/\/t\.me\/[a-zA-Z0-9_]+\/?$", ErrorMessage = "Ссылка должна быть в формате https://t.me/username")]
        public string TelegramUrl { get; set; } = string.Empty;
    }
}