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

        public async Task<(List<ShortUrl> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize, string? search = null, string? sortBy = null, string? sortDirection = null, string? status = null)
        {
            var query = _context.ShortUrls
                .Where(su => su.UserId == userId && !su.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(su => 
                    su.OriginalUrl.ToLower().Contains(lowerSearch) || 
                    su.ShortCode.ToLower().Contains(lowerSearch));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var lowerStatus = status.ToLower();
                if (lowerStatus == "active")
                {
                    query = query.Where(su => su.IsActive);
                }
                else if (lowerStatus == "inactive")
                {
                    query = query.Where(su => !su.IsActive);
                }
            }

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortDirection);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        private IQueryable<ShortUrl> ApplySorting(IQueryable<ShortUrl> query, string? sortBy, string? sortDirection)
        {
            var isAsc = sortDirection?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "originalurl" => isAsc ? query.OrderBy(u => u.OriginalUrl) : query.OrderByDescending(u => u.OriginalUrl),
                "shorturl" or "shortcode" => isAsc ? query.OrderBy(u => u.ShortCode) : query.OrderByDescending(u => u.ShortCode),
                "clickcount" => isAsc ? query.OrderBy(u => u.ClickCount) : query.OrderByDescending(u => u.ClickCount),
                "createdat" => isAsc ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                "status" => isAsc ? query.OrderBy(u => u.IsActive) : query.OrderByDescending(u => u.IsActive),
                _ => query.OrderByDescending(u => u.CreatedAt) // Default sorting
            };
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
