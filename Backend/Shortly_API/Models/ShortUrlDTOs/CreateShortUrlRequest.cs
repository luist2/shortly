using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Models.ShortUrlDTOs
{
    public class CreateShortUrlRequest
    {
        [Required]
        [MaxLength(2048)]
        [Url(ErrorMessage = "The provided URL is not valid.")]
        public string OriginalUrl { get; set; } = string.Empty;

        // Opcional: solo se valida para usuarios autenticados
        public int? ExpiresInHours { get; set; }
    }
}
