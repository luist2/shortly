using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shortly_API.Entities;
using Shortly_API.Middleware;
using Shortly_API.Models;
using System.Security.Claims;
using Shortly_API.Services;

namespace Shortly_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly string _refreshTokenCookieName;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _refreshTokenCookieName = configuration.GetValue<string>("CookieSettings:RefreshTokenCookieName") ?? "refreshToken";
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserResponseDTO>> Register(UserDTO request)
        {
            var response = await _authService.RegisterAsync(request);
            if (response is null)
            {
                return Conflict("Email already exists");
            }

            return Ok(response);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenResponseDTO>> Login(UserDTO request)
        {

            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return Unauthorized("Invalid email or password");
            }

            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiry);

            return Ok(new { AccessToken = result.AccessToken });
        }

        [HttpPost("refresh-tokens")]
        [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenResponseDTO>> RefreshTokens(RefreshTokenRequestDTO request)
        {
            var refreshToken = Request.Cookies[_refreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                 return Unauthorized("No refresh token found in cookie");
            }

            request.RefreshToken = refreshToken;

            var result = await _authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh token");
            }

            // Solo actualizar la cookie si el refresh token fue rotado
            if (result.RefreshToken != refreshToken)
            {
                SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiry);
            }

            return Ok(new { AccessToken = result.AccessToken });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized();
            }

            await _authService.LogoutAsync(Guid.Parse(userId));

            Response.Cookies.Delete(_refreshTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });

            return Ok(new { message = "Logged out successfully" });
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expiry)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiry,
                // None es necesario porque frontend (Netlify) y backend (Render) estan en dominios distintos.
                SameSite = SameSiteMode.None,
                Secure = true // Obligatorio cuando SameSite = None
            };
            Response.Cookies.Append(_refreshTokenCookieName, refreshToken, cookieOptions);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public IActionResult AuthenticationOnlyEndpoint()
        {
            return Ok("You are authenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are an admin");
        }
    }
}
