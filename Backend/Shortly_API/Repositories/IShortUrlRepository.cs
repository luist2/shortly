using Shortly_API.Entities;

namespace Shortly_API.Repositories
{
    public interface IShortUrlRepository
    {
        Task CreateAsync(ShortUrl shortUrl);
        Task<ShortUrl?> GetByShortCodeAsync(string shortCode);
        Task<List<ShortUrl>> GetByUserIdAsync(Guid userId);
        Task<bool> ExistsAsync(string shortCode);
        Task IncrementClickCountAsync(ShortUrl shortUrl);
        Task SaveChangesAsync();
    }
}
