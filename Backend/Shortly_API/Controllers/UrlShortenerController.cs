using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shortly_API.Middleware;
using Shortly_API.Models;
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
        [ProducesResponseType(typeof(ShortUrlResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(typeof(PagedResult<ShortUrlResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<ShortUrlResponse>>> GetUserUrls([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string? sortDirection = null, [FromQuery] string? status = null)
        {
            var userId = GetUserIdFromToken();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50; // Limitar tamaño máximo de página

            var urls = await _urlShortenerService.GetUserUrlsAsync(userId.Value, page, pageSize, search, sortBy, sortDirection, status);
            return Ok(urls);
        }

        // GET /api/urlshortener/urls/{shortCode}
        [HttpGet("urls/{shortCode}")]
        [Authorize] // Solo usuarios autenticados pueden ver stats
        [ProducesResponseType(typeof(ShortUrlStatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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