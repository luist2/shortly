using Microsoft.EntityFrameworkCore;
using Shortly_API.Data;
using Shortly_API.Entities;
using Shortly_API.Models.ShortUrlDTOs;
using Shortly_API.Repositories;

namespace Shortly_API.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly IShortUrlRepository _repository;
        private readonly ILogger<UrlShortenerService> _logger;
        private readonly IConfiguration _config;

        public UrlShortenerService(IShortUrlRepository repository, ILogger<UrlShortenerService> logger, IConfiguration config)
        {
            _repository = repository;
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
            int retries = 0;

            do
            {
                if(retries++ > 5)
                {
                    throw new Exception("Failed to generate a unique short code after multiple attempts.");
                }
                shortCode = ShortCodeGenerator.Generate(8);

            } while (await _repository.ExistsAsync(shortCode));

            var shortUrl = new ShortUrl
            {
                ShortCode = shortCode,
                OriginalUrl = originalUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.CreateAsync(shortUrl);
            await _repository.SaveChangesAsync();

            return new ShortUrlResponse
            {
                ShortCode = shortUrl.ShortCode,
                OriginalUrl = shortUrl.OriginalUrl,
                ShortUrl = $"{baseDomain}/{shortUrl.ShortCode}",
                CreatedAt = shortUrl.CreatedAt,
                ClickCount = shortUrl.ClickCount
            };
        }

        public async Task<bool> DeleteShortUrlAsync(string shortCode, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                throw new ArgumentException("Short code cannot be null or empty.");
            }
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.");
            }

            var shortUrl = await _repository.GetByShortCodeAndUserIdAsync(shortCode, userId);

            if (shortUrl == null)
            {
                _logger.LogWarning("User {UserId} attempted to delete non-existent or unauthorized short URL {ShortCode}.", userId, shortCode);
                throw new KeyNotFoundException("Short URL not found or does not belong to the user.");
            }

            if (!shortUrl.IsActive)
            {
                _logger.LogInformation("Short URL {ShortCode} is already inactive.", shortCode);
                return false; // Ya está inactivo
            }

            shortUrl.IsActive = false;
            await _repository.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted short URL {ShortCode}.", userId, shortCode);
            return true;
        }

        public async Task<string> GetOriginalUrlAsync(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                throw new ArgumentException("Short code cannot be null or empty.");
            }

            var shortUrl = await _repository.GetByShortCodeAsync(shortCode);

            if (shortUrl == null)
            {
                throw new KeyNotFoundException("Short URL not found.");
            }

            // Verificar expiración (si aplica)
            if (shortUrl.ExpiresAt.HasValue && shortUrl.ExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Short URL with code {ShortCode} has expired at {ExpiresAt}.", shortCode, shortUrl.ExpiresAt);
                throw new InvalidOperationException("This short URL has expired.");
            }

            // Tracking de clicks
            await _repository.IncrementClickCountAsync(shortUrl);

            _logger.LogInformation("Short URL with code {ShortCode} accessed. Total clicks: {ClickCount}.", shortCode, shortUrl.ClickCount);

            return shortUrl.OriginalUrl;
        }

        public async Task<ShortUrlStatsResponse> GetUrlStatsAsync(string shortCode, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                throw new ArgumentException("Short code cannot be null or empty.");
            }
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.");
            }

            var shortUrl = await _repository.GetByShortCodeAndUserIdAsync(shortCode, userId);

            if (shortUrl == null)
            {
                throw new KeyNotFoundException("Short URL not found or does not belong to the user.");
            }

            var response = new ShortUrlStatsResponse
            {
                ShortCode = shortUrl.ShortCode,
                OriginalUrl = shortUrl.OriginalUrl,
                CreatedAt = shortUrl.CreatedAt,
                ClickCount = shortUrl.ClickCount,
                LastAccessedAt = shortUrl.LastAccessedAt,
                ExpiresAt = shortUrl.ExpiresAt,
                IsActive = shortUrl.IsActive
            };

            _logger.LogInformation("User {UserId} retrieved stats for short URL {ShortCode}.", userId, shortCode);

            return response;
        }

        public async Task<List<ShortUrlResponse>> GetUserUrlsAsync(Guid userId)
        {
            if (userId == Guid.Empty){
                throw new ArgumentException("User ID cannot be empty.");
            }

            var baseDomain = _config["AppSettings:BaseDomain"];
            var shortUrls = await _repository.GetByUserIdAsync(userId);

            var response = shortUrls.Select(su => new ShortUrlResponse
            {
                ShortCode = su.ShortCode,
                OriginalUrl = su.OriginalUrl,
                ShortUrl = $"{baseDomain}/{su.ShortCode}",
                CreatedAt = su.CreatedAt,
                ClickCount = su.ClickCount
            }).ToList();

            _logger.LogInformation("User {UserId} retrieved {Count} short URLs.", userId, response.Count);

            return response;
        }
    }
}
