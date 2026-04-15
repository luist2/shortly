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
        private readonly IConfiguration _configuration;

        public RedirectController(
            IUrlShortenerService urlShortenerService,
            ILogger<RedirectController> logger,
            IConfiguration configuration)
        {
            _urlShortenerService = urlShortenerService;
            _logger = logger;
            _configuration = configuration;
        }

        private string BuildFrontendStatusUrl(string reason, string shortCode)
        {
            var frontendBaseUrl = _configuration["GeneralSettings:FrontendBaseUrl"];
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                throw new InvalidOperationException("GeneralSettings:FrontendBaseUrl is required.");
            }

            var normalizedBaseUrl = frontendBaseUrl.TrimEnd('/');
            return $"{normalizedBaseUrl}/link-status?reason={Uri.EscapeDataString(reason)}&code={Uri.EscapeDataString(shortCode)}";
        }

        private IActionResult RedirectToFrontendStatus(string reason, string shortCode)
        {
            try
            {
                var targetUrl = BuildFrontendStatusUrl(reason, shortCode);
                return Redirect(targetUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to build frontend error redirect URL.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while processing your request." });
            }
        }

        // GET /{shortCode}
        [HttpGet("{shortCode}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
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
                return RedirectToFrontendStatus("invalid", shortCode);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Short URL not found: {ShortCode}", shortCode);
                return RedirectToFrontendStatus("not-found", shortCode);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Short URL with code {ShortCode} is invalid or expired: {Message}", shortCode, ex.Message);
                return RedirectToFrontendStatus("expired", shortCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while redirecting for short code: {ShortCode}", shortCode);
                return RedirectToFrontendStatus("server-error", shortCode);
            }
        }
    }
}
