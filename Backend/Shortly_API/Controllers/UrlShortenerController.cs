using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shortly_API.Models.ShortUrlDTOs;
using Shortly_API.Services;
using System.Security.Claims;

namespace Shortly_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlShortenerController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;

        public UrlShortenerController(IUrlShortenerService urlShortenerService)
        {
            _urlShortenerService = urlShortenerService;
        }

        // Método para obtener el UserId del token (si existe)
        private Guid? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }

        // POST /api/urlshortener/urls
        // Endpoint que permite tanto usuarios autenticados como anónimos
        [HttpPost("urls")]
        [AllowAnonymous] // Permite acceso sin autenticación
        public async Task<ActionResult<ShortUrlResponse>> CreateShortUrl([FromBody] CreateShortUrlRequest request)
        {
            var userId = GetUserIdFromToken();

            ShortUrlResponse result;

            if (userId.HasValue)
            {
                // Usuario autenticado: URL sin expiración
                result = await _urlShortenerService.CreateShortUrlAsync(request.OriginalUrl, userId.Value);
            }
            else
            {
                // Usuario anónimo: URL con expiración de 24 horas
                result = await _urlShortenerService.CreateShortUrlAsync(request.OriginalUrl);
            }

            return Ok(result);
        }

        // GET /api/urlshortener/urls
        [HttpGet("urls")]
        [Authorize] // Solo usuarios autenticados pueden ver sus URLs
        public async Task<ActionResult<List<ShortUrlResponse>>> GetUserUrls()
        {
            var userId = GetUserIdFromToken();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var urls = await _urlShortenerService.GetUserUrlsAsync(userId.Value);
            return Ok(urls);
        }

        // GET /api/urlshortener/urls/{shortCode}
        [HttpGet("urls/{shortCode}")]
        [Authorize] // Solo usuarios autenticados pueden ver stats
        public async Task<ActionResult<ShortUrlStatsResponse>> GetUrlStats(string shortCode)
        {
            var userId = GetUserIdFromToken();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var stats = await _urlShortenerService.GetUrlStatsAsync(shortCode, userId.Value);
            return Ok(stats);
        }

        // DELETE /api/urlshortener/urls/{shortCode}
        [HttpDelete("urls/{shortCode}")]
        [Authorize] // Solo usuarios autenticados pueden eliminar URLs
        public async Task<IActionResult> DeleteShortUrl(string shortCode)
        {
            var userId = GetUserIdFromToken();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var result = await _urlShortenerService.DeleteShortUrlAsync(shortCode, userId.Value);
            return NoContent();
        }

    }
}