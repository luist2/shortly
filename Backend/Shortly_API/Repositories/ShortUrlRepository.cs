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
                    !su.IsDeleted);
        }

        public async Task<ShortUrl?> GetByShortCodeAsync(string shortCode)
        {
            return await _context.ShortUrls
                .FirstOrDefaultAsync(su => su.ShortCode == shortCode && su.IsActive && !su.IsDeleted);
        }

        public async Task<(List<ShortUrl> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize)
        {
            var query = _context.ShortUrls
                .Where(su => su.UserId == userId && !su.IsDeleted);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(su => su.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
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
