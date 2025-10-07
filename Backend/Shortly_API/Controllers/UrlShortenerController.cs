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

        // POST /api/urls
        [HttpPost("urls")]
        public async Task<ActionResult<ShortUrlResponse>> CreateShortUrl([FromBody] CreateShortUrlRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtener el Id del usuario desde el token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            // Checkear que el claim no sea nulo y convertirlo a Guid
            var userId = Guid.Parse(userIdClaim.Value);

            // Llamar al servicio para crear la URL corta
            var result = await _urlShortenerService.CreateShortUrlAsync(request.OriginalUrl, userId);

            return Ok(result);
        }

        // GET /api/urls
        [HttpGet("urls")]
        public async Task<ActionResult<List<ShortUrlResponse>>> GetUserUrls()
        {
            // Obtener el Id del usuario desde el token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            // Checkear que el claim no sea nulo y convertirlo a Guid
            var userId = Guid.Parse(userIdClaim.Value);

            var urls = await _urlShortenerService.GetUserUrlsAsync(userId);
            return Ok(urls);
        }
    }
}
