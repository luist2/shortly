using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> RedirectToOriginalUrl(string shortCode)
        {
            try
            {
                var originalUrl = await _urlShortenerService.GetOriginalUrlAsync(shortCode);
                
                return RedirectPermanent(originalUrl);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Short URL not found: {ShortCode}", shortCode);
                return NotFound(new { message = "Short URL not found." });

            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Short URL with code {ShortCode} is invalid or expired: {Message}", shortCode, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while redirecting for short code: {ShortCode}", shortCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request." });
            }
        }
    }
}
