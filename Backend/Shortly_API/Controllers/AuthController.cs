using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shortly_API.Middleware;
using Shortly_API.Models;
using Shortly_API.Services;
using System.Security.Claims;

namespace Shortly_API.Controllers
{
    /// <summary>
    /// Handles account registration, login, token refresh, and logout operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly string _refreshTokenCookieName;

        /// <summary>
        /// Creates a new auth controller instance.
        /// </summary>
        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _refreshTokenCookieName = configuration.GetValue<string>("CookieSettings:RefreshTokenCookieName") ?? "refreshToken";
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">User registration payload.</param>
        /// <returns>Created user information or conflict when email is already used.</returns>
        [HttpPost("register")]
        [EnableRateLimiting("auth-strict")]
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

        /// <summary>
        /// Authenticates user credentials and starts a new session.
        /// </summary>
        /// <param name="request">User login payload.</param>
        /// <returns>Access token in body and refresh token in HttpOnly cookie.</returns>
        [HttpPost("login")]
        [EnableRateLimiting("auth-strict")]
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

        /// <summary>
        /// Refreshes the access token using the refresh token stored in cookie.
        /// </summary>
        /// <returns>New access token and rotated refresh cookie when applicable.</returns>
        [HttpPost("refresh-tokens")]
        [EnableRateLimiting("auth-refresh")]
        [ProducesResponseType(typeof(TokenResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TokenResponseDTO>> RefreshTokens()
        {
            var refreshToken = Request.Cookies[_refreshTokenCookieName];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No refresh token found in cookie");
            }

            var result = await _authService.RefreshTokensAsync(refreshToken);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh token");
            }

            // Only rewrite cookie when refresh token rotation occurred.
            if (result.RefreshToken != refreshToken)
            {
                SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiry);
            }

            return Ok(new { AccessToken = result.AccessToken });
        }

        /// <summary>
        /// Logs out only the current session.
        /// </summary>
        /// <returns>Success when current session is revoked and cookie deleted.</returns>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized();
            }

            var refreshToken = Request.Cookies[_refreshTokenCookieName];
            await _authService.LogoutAsync(Guid.Parse(userId), refreshToken);

            DeleteRefreshTokenCookie();
            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Logs out all active sessions for the authenticated user.
        /// </summary>
        /// <returns>Total revoked sessions and deletion of current refresh cookie.</returns>
        [Authorize]
        [HttpPost("logout-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized();
            }

            var revokedSessions = await _authService.LogoutAllAsync(Guid.Parse(userId));
            DeleteRefreshTokenCookie();
            return Ok(new { message = "Logged out from all sessions", revokedSessions });
        }

        /// <summary>
        /// Writes the refresh token as secure HttpOnly cookie.
        /// </summary>
        private void SetRefreshTokenCookie(string refreshToken, DateTime expiry)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiry,
                SameSite = SameSiteMode.None,
                Secure = true
            };
            Response.Cookies.Append(_refreshTokenCookieName, refreshToken, cookieOptions);
        }

        /// <summary>
        /// Deletes the refresh token cookie using matching secure cookie settings.
        /// </summary>
        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete(_refreshTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
        }

        /// <summary>
        /// Health endpoint to verify JWT authentication.
        /// </summary>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public IActionResult AuthenticationOnlyEndpoint()
        {
            return Ok("You are authenticated");
        }

        /// <summary>
        /// Sample endpoint restricted to users with Admin role.
        /// </summary>
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
