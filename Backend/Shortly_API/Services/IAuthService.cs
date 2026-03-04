using Shortly_API.Models;

namespace Shortly_API.Services
{
    /// <summary>
    /// Defines authentication operations for registration, login, token refresh, and session revocation.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">User registration payload.</param>
        /// <returns>The registered user data, or null if email already exists.</returns>
        Task<UserResponseDTO?> RegisterAsync(UserDTO request);

        /// <summary>
        /// Validates credentials and creates a new authenticated session.
        /// </summary>
        /// <param name="request">User login payload.</param>
        /// <returns>Issued access/refresh tokens, or null when credentials are invalid.</returns>
        Task<TokenResponseDTO?> LoginAsync(UserDTO request);

        /// <summary>
        /// Renews auth tokens using the refresh token received from cookie.
        /// </summary>
        /// <param name="refreshToken">Raw refresh token value from HttpOnly cookie.</param>
        /// <returns>New access token and optional rotated refresh token, or null when refresh is invalid.</returns>
        Task<TokenResponseDTO?> RefreshTokensAsync(string refreshToken);

        /// <summary>
        /// Revokes only the current session identified by user and refresh token.
        /// </summary>
        /// <param name="userId">Authenticated user id.</param>
        /// <param name="refreshToken">Raw refresh token value from HttpOnly cookie.</param>
        Task LogoutAsync(Guid userId, string? refreshToken);

        /// <summary>
        /// Revokes all active sessions for the specified user.
        /// </summary>
        /// <param name="userId">Authenticated user id.</param>
        /// <returns>Total number of sessions revoked.</returns>
        Task<int> LogoutAllAsync(Guid userId);
    }
}
