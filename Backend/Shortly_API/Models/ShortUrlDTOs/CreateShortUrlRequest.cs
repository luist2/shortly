using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Models.ShortUrlDTOs
{
    public class CreateShortUrlRequest
    {
        [Required]
        [MaxLength(2048)]
        [Url(ErrorMessage = "The provided URL is not valid.")]
        public string OriginalUrl { get; set; } = string.Empty;

        // Obligatorio: la expiración de la URL
        [Required(ErrorMessage = "Expiration time is required.")]
        [AllowedValues(1, 24, 72, 168, 336, ErrorMessage = "Allowed expiration values are 1, 24, 72, 168, or 336 hours.")]
        public int ExpiresInHours { get; set; }
    }
}
