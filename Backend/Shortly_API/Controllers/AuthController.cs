using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shortly_API.Entities;
using Shortly_API.Models;
using Shortly_API.Services;

namespace Shortly_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
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
        public async Task<ActionResult<TokenResponseDTO>> Login(UserDTO request)
        {

            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                return Unauthorized("Invalid email or password");
            }

            return Ok(result);
        }
        [HttpPost("refresh-tokens")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshTokens(RefreshTokenRequestDTO request)
        {
            var result = await _authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh token");
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticationOnlyEndpoint()
        {
            return Ok("You are authenticated");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are an admin");
        }
    }
}
