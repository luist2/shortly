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

        Task<List<ShortUrlResponse>> GetUserUrlsAsync(Guid userId);
        Task<ShortUrlStatsResponse> GetUrlStatsAsync(string shortCode, Guid userId);
        Task<bool> DeleteShortUrlAsync(string shortCode, Guid userId);
    }
}
