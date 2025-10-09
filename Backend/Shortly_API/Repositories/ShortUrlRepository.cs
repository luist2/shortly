using Microsoft.EntityFrameworkCore;
using Shortly_API.Data;
using Shortly_API.Entities;

namespace Shortly_API.Repositories
{
    public class ShortUrlRepository : IShortUrlRepository
    {
        private readonly AppDbContext _context;

        public ShortUrlRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(ShortUrl shortUrl)
        {
            await _context.ShortUrls.AddAsync(shortUrl);
        }

        public async Task<bool> ExistsAsync(string shortCode)
        {
            return await _context.ShortUrls.AnyAsync(su => su.ShortCode == shortCode);
        }

        public async Task<ShortUrl?> GetByShortCodeAndUserIdAsync(string shortCode, Guid userId)
        {
            return await _context.ShortUrls
                .FirstOrDefaultAsync(su =>
                    su.ShortCode == shortCode &&
                    su.UserId == userId &&
                    su.IsActive);
        }

        public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode)
        {
            return await _context.ShortUrls
                .FirstOrDefaultAsync(su => su.ShortCode == shortCode && su.IsActive);
        }

        public Task<List<ShortUrl>> GetByUserIdAsync(Guid userId)
        {
            // Devolver URLs activas y ordenadas por fecha de creación descendente
            return _context.ShortUrls
                .Where(su => su.UserId == userId && su.IsActive)
                .OrderByDescending(su => su.CreatedAt)
                .ToListAsync();
        }

        public async Task IncrementClickCountAsync(ShortUrl shortUrl)
        {
            shortUrl.ClickCount++;
            shortUrl.LastAccessedAt = DateTime.UtcNow;
            _context.ShortUrls.Update(shortUrl);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
