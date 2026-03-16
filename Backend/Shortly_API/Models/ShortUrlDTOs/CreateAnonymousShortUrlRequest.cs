using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Models.ShortUrlDTOs
{
    public class CreateAnonymousShortUrlRequest
    {
        [Required]
        [MaxLength(2048)]
        [Url(ErrorMessage = "The provided URL is not valid.")]
        public string OriginalUrl { get; set; } = string.Empty;
    }
}
