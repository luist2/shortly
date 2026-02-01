using Shortly_API.Entities;

namespace Shortly_API.Repositories
{
    public interface IShortUrlRepository
    {
        Task CreateAsync(ShortUrl shortUrl);
        Task<ShortUrl?> GetByShortCodeAsync(string shortCode);
        Task<ShortUrl?> GetByShortCodeAndUserIdAsync(string shortCode, Guid userId);
        Task<(List<ShortUrl> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize, string? search = null, string? sortBy = null, string? sortDirection = null, string? status = null);
        Task<bool> ExistsAsync(string shortCode);
        Task IncrementClickCountAsync(ShortUrl shortUrl);
        Task SaveChangesAsync();
    }
}
