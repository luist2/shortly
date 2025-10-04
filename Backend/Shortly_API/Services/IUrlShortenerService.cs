using Shortly_API.Models.ShortUrlDTOs;

namespace Shortly_API.Services
{
    public interface IUrlShortenerService
    {
        // Crea una URL corta para la URL original proporcionada y asocia la URL corta con el usuario especificado.
        Task<ShortUrlResponse> CreateShortUrlAsync(string originalUrl, Guid userId);
    }
}
