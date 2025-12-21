namespace Shortly_API.Models.ShortUrlDTOs
{
    public class ShortUrlResponse
    {
        public string ShortCode { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty; // dominio.com/{shortCode}
        public string OriginalUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ClickCount { get; set; }
        public bool IsActive { get; set; }
    }
}
