namespace Shortly_API.Models.ShortUrlDTOs
{
    public class ShortUrlStatsResponse
    {
        public string ShortCode { get; set; } = string.Empty;
        public string ShortUrl { get; set; } = string.Empty; // dominio.com/{shortCode}
        public string OriginalUrl { get; set; } = string.Empty;
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
