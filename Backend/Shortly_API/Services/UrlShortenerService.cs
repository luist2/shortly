using Shortly_API.Entities;
using Shortly_API.Models.ShortUrlDTOs;
using Shortly_API.Repositories;
using Shortly_API.Utils;

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

        // Validaciones básicas
        private void ValidateShortCode(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
            {
                throw new ArgumentException("Short code cannot be null or empty.");
            }
        }

        private void ValidateUserId(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty.");
            }
        }

        private void ValidateOriginalUrl(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
            {
                throw new ArgumentException("Original URL cannot be null or empty.");
            }

            if (!Uri.IsWellFormedUriString(originalUrl, UriKind.Absolute))
            {
                throw new ArgumentException("Original URL is not valid.");
            }
        }

        // Método para usuarios anónimos
        public async Task<ShortUrlResponse> CreateShortUrlAsync(string originalUrl)
        {
            ValidateOriginalUrl(originalUrl);

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
                    _logger.LogError("Failed to generate a unique short code after multiple attempts.");
                    throw new Exception("Failed to generate a unique short code after multiple attempts.");
                }

                shortCode = ShortCodeGenerator.Generate();

            } while (await _repository.ExistsAsync(shortCode));

            var shortUrl = new ShortUrl
            {
                ShortCode = shortCode,
                OriginalUrl = originalUrl,
                UserId = null, // Usuario anónimo
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // Expiración en 24 horas
                IsActive = true
            };

            await _repository.CreateAsync(shortUrl);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Created short URL {ShortCode} for anonymous user.", shortCode);

            return new ShortUrlResponse
            {
                ShortCode = shortUrl.ShortCode,
                OriginalUrl = shortUrl.OriginalUrl,
                ShortUrl = $"{baseDomain}/{shortUrl.ShortCode}",
                CreatedAt = shortUrl.CreatedAt,
                ClickCount = shortUrl.ClickCount
            };
        }

        // Método para usuarios autenticados
        public async Task<ShortUrlResponse> CreateShortUrlAsync(string originalUrl, Guid userId)
        {
            ValidateOriginalUrl(originalUrl);
            ValidateUserId(userId);

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
                    _logger.LogError("Failed to generate a unique short code after multiple attempts.");
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

            _logger.LogInformation("Created short URL {ShortCode} for user {UserId}.", shortCode, userId);

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
            ValidateShortCode(shortCode);
            ValidateUserId(userId);

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
            ValidateShortCode(shortCode);

            var shortUrl = await _repository.GetByShortCodeAsync(shortCode);

            if (shortUrl == null)
            {
                _logger.LogWarning("Short URL with code {ShortCode} not found or inactive.", shortCode);
                throw new KeyNotFoundException("Short URL not found.");
            }

            // Verificar expiración (si aplica)
            if (shortUrl.ExpiresAt.HasValue && shortUrl.ExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Short URL with code {ShortCode} has expired at {ExpiresAt}.", shortCode, shortUrl.ExpiresAt);

                // Desactivar la URL expirada
                shortUrl.IsActive = false;
                await _repository.SaveChangesAsync();

                throw new InvalidOperationException("This short URL has expired.");
            }

            // Tracking de clicks
            await _repository.IncrementClickCountAsync(shortUrl);

            _logger.LogInformation("Short URL with code {ShortCode} accessed. Total clicks: {ClickCount}.", shortCode, shortUrl.ClickCount);

            return shortUrl.OriginalUrl;
        }

        public async Task<ShortUrlStatsResponse> GetUrlStatsAsync(string shortCode, Guid userId)
        {
            ValidateShortCode(shortCode);
            ValidateUserId(userId);

            var shortUrl = await _repository.GetByShortCodeAndUserIdAsync(shortCode, userId);

            if (shortUrl == null)
            {
                _logger.LogWarning("User {UserId} attempted to access stats for non-existent or unauthorized short URL {ShortCode}.", userId, shortCode);
                throw new KeyNotFoundException("Short URL not found or does not belong to the user.");
            }

            var baseDomain = _config["AppSettings:BaseDomain"];

            var response = new ShortUrlStatsResponse
            {
                ShortCode = shortUrl.ShortCode,
                ShortUrl = $"{baseDomain}/{shortUrl.ShortCode}",
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
            ValidateUserId(userId);

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
