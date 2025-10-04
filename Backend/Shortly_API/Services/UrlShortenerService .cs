using Microsoft.EntityFrameworkCore;
using Shortly_API.Data;
using Shortly_API.Entities;
using Shortly_API.Models.ShortUrlDTOs;

namespace Shortly_API.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UrlShortenerService> _logger;
        private readonly IConfiguration _config;

        public UrlShortenerService(AppDbContext context, ILogger<UrlShortenerService> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task<ShortUrlResponse> CreateShortUrlAsync(string originalUrl, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                throw new ArgumentException("Original URL cannot be null or empty.");
            }
            if(!Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            {
                throw new ArgumentException("Original URL is not valid.");
            }

            // Evitar acortar una URL que apunte a la propia aplicación
            var baseDomain = _config["AppSettings:BaseDomain"];
            if(originalUrl.StartsWith(baseDomain, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot shorten a URL that points to the same domain as the application.");
            }

            // Lógica para generar un código corto único
            string shortCode;
            bool exists;
            int retries = 0;

            do
            {
                if(retries++ > 5)
                {
                    throw new Exception("Failed to generate a unique short code after multiple attempts.");
                }
                shortCode = ShortCodeGenerator.Generate(8);
                exists = await _context.ShortUrls.AnyAsync(su => su.ShortCode == shortCode);

            } while (exists);

            var shortUrlEntity = new ShortUrl
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ShortUrls.Add(shortUrlEntity);
            await _context.SaveChangesAsync();

            return new ShortUrlResponse
            {
                ShortCode = shortUrlEntity.ShortCode,
                OriginalUrl = originalUrl,
                ShortUrl = $"{baseDomain}/{shortUrlEntity.ShortCode}",
                CreatedAt = shortUrlEntity.CreatedAt,
                ClickCount = shortUrlEntity.ClickCount
            };
        }
    }
}
