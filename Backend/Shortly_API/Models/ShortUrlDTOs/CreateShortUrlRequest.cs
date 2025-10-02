using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Models.ShortUrlDTOs
{
    public class CreateShortUrlRequest
    {
        [Required]
        [MaxLength(2048)]
        [Url(ErrorMessage = "La URL proporcionada no es válida.")]
        public string OriginalUrl { get; set; } = string.Empty;

        // Opcional: permitir expiración definida por el usuario
        public DateTime? ExpiresAt { get; set; }
    }
}
