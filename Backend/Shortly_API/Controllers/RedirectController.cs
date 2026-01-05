using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shortly_API.Middleware;
using Shortly_API.Services;

namespace Shortly_API.Controllers
{
    [ApiController]
    public class RedirectController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;
        private readonly ILogger<RedirectController> _logger;

        public RedirectController(IUrlShortenerService urlShortenerService, ILogger<RedirectController> logger)
        {
            _urlShortenerService = urlShortenerService;
            _logger = logger;
        }

        // GET /{shortCode}
        [HttpGet("{shortCode}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            try
            {
                var originalUrl = await _urlShortenerService.GetOriginalUrlAsync(shortCode);
                return Redirect(originalUrl);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid short code provided: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Short URL not found: {ShortCode}", shortCode);
                return NotFound(new { message = "Short URL not found or inactive." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Short URL with code {ShortCode} is invalid or expired: {Message}", shortCode, ex.Message);
                return StatusCode(StatusCodes.Status410Gone, new { message = "This short URL has expired." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while redirecting for short code: {ShortCode}", shortCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request." });
            }
        }
    }
}
