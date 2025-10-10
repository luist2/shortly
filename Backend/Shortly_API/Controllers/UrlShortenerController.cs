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
    [Authorize]
    public class UrlShortenerController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;

        public UrlShortenerController(IUrlShortenerService urlShortenerService)
        {
            _urlShortenerService = urlShortenerService;
        }

        // Método para obtener el UserId del token
        private Guid GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID in token.");
            }
            return userId;
        }

        // POST /api/urls
        [HttpPost("urls")]
        public async Task<ActionResult<ShortUrlResponse>> CreateShortUrl([FromBody] CreateShortUrlRequest request)
        {
            var userId = GetUserIdFromToken();
            var result = await _urlShortenerService.CreateShortUrlAsync(request.OriginalUrl, userId);

            return Ok(result);
        }

        // GET /api/urls
        [HttpGet("urls")]
        public async Task<ActionResult<List<ShortUrlResponse>>> GetUserUrls()
        {
            var userId = GetUserIdFromToken();
            var urls = await _urlShortenerService.GetUserUrlsAsync(userId);

            return Ok(urls);
        }

        // GET /api/urls/{shortCode}
        [HttpGet("urls/{shortCode}")]
        public async Task<ActionResult<ShortUrlStatsResponse>> GetUrlStats(string shortCode)
        {
            var userId = GetUserIdFromToken();
            var stats = await _urlShortenerService.GetUrlStatsAsync(shortCode, userId);

            return Ok(stats);
        }

        // DELETE /api/urls/{shortCode}
        [HttpDelete("urls/{shortCode}")]
        public async Task<IActionResult> DeleteShortUrl(string shortCode)
        {
            var userId = GetUserIdFromToken();
            var result = await _urlShortenerService.DeleteShortUrlAsync(shortCode, userId);

            return NoContent();
        }

    }
}