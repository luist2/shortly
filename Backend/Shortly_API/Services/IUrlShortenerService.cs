using Shortly_API.Models;
using Shortly_API.Models.ShortUrlDTOs;

namespace Shortly_API.Services
{
    public interface IUrlShortenerService
    {
        // Crea una URL corta para la URL original proporcionada y asocia la URL corta con el usuario especificado.
        Task<ShortUrlResponse> CreateShortUrlAsync(string originalUrl, Guid userId);

        // Método para usuarios anónimos
        Task<ShortUrlResponse> CreateShortUrlAsync(string originalUrl);

        // Recupera la URL original asociada con el código corto proporcionado.
        Task<string> GetOriginalUrlAsync(string shortCode);

        Task<PagedResult<ShortUrlResponse>> GetUserUrlsAsync(Guid userId, int page, int pageSize, string? search = null, string? sortBy = null, string? sortDirection = null, string? status = null);
        Task<ShortUrlStatsResponse> GetUrlStatsAsync(string shortCode, Guid userId);
        Task<bool> DeleteShortUrlAsync(string shortCode, Guid userId);
    }
}
