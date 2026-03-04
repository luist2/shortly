using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shortly_API.Data;
using Shortly_API.Entities;
using Shortly_API.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Shortly_API.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
    {
        private const string RevokeReasonLimitExceeded = "limit_exceeded";
        private const string RevokeReasonRotated = "rotated";
        private const string RevokeReasonUserLogout = "user_logout";
        private const string RevokeReasonUserLogoutAll = "user_logout_all";
        private const int DefaultMaxActiveSessions = 3;

        /// <summary>
        /// Authenticates a user, enforces the max active session policy, and issues a new token pair.
        /// </summary>
        public async Task<TokenResponseDTO?> LoginAsync(UserDTO request)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var now = DateTime.UtcNow;

            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var maxActiveSessions = GetMaxActiveSessions();
            var activeSessions = await context.UserSessions
                .Where(s => s.UserId == user.Id && s.RevokedAtUtc == null && s.ExpiresAtUtc > now)
                .OrderBy(s => s.CreatedAtUtc)
                .ToListAsync();

            var sessionsToRevoke = Math.Max(0, activeSessions.Count - maxActiveSessions + 1);
            foreach (var session in activeSessions.Take(sessionsToRevoke))
            {
                RevokeSession(session, now, RevokeReasonLimitExceeded);
            }

            var (newSession, newRefreshToken) = await CreateSessionAsync(user.Id, now);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return BuildTokenResponse(user, newSession.ExpiresAtUtc, newRefreshToken);
        }

        /// <summary>
        /// Registers a new user if the email is not already in use.
        /// </summary>
        public async Task<UserResponseDTO?> RegisterAsync(UserDTO request)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return null;
            }

            var user = new User
            {
                Email = request.Email,
                Role = configuration.GetValue<string>("GeneralSettings:DefaultRole") ?? "User"
            };

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email
            };
        }

        /// <summary>
        /// Refreshes access/refresh tokens using the refresh token from the HttpOnly cookie.
        /// </summary>
        public async Task<TokenResponseDTO?> RefreshTokensAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var refreshTokenHash = HashRefreshToken(refreshToken);
            var now = DateTime.UtcNow;
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var currentSession = await context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.RefreshTokenHash == refreshTokenHash);

            if (currentSession is null || !IsSessionActive(currentSession, now))
            {
                return null;
            }

            currentSession.LastUsedAtUtc = now;
            var user = currentSession.User;

            var timeRemaining = currentSession.ExpiresAtUtc - now;
            var rotationHours = configuration.GetValue<int>("JwtSettings:RefreshTokenRotationHours");
            var shouldRotateRefreshToken = timeRemaining.TotalHours < rotationHours;

            if (!shouldRotateRefreshToken)
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return BuildTokenResponse(user, currentSession.ExpiresAtUtc, refreshToken);
            }

            RevokeSession(currentSession, now, RevokeReasonRotated);
            var (rotatedSession, rotatedRefreshToken) = await CreateSessionAsync(user.Id, now);
            currentSession.ReplacedBySessionId = rotatedSession.Id;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return BuildTokenResponse(user, rotatedSession.ExpiresAtUtc, rotatedRefreshToken);
        }

        /// <summary>
        /// Generates a cryptographically secure random refresh token.
        /// </summary>
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Produces a unique refresh token + hash pair for persistence.
        /// </summary>
        private async Task<(string rawRefreshToken, string refreshTokenHash)> GenerateUniqueRefreshTokenPairAsync()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var rawToken = GenerateRefreshToken();
                var tokenHash = HashRefreshToken(rawToken);
                var exists = await context.UserSessions.AnyAsync(s => s.RefreshTokenHash == tokenHash);
                if (!exists)
                {
                    return (rawToken, tokenHash);
                }
            }

            var fallbackRawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return (fallbackRawToken, HashRefreshToken(fallbackRawToken));
        }

        /// <summary>
        /// Creates and tracks a new user session row with hashed refresh token.
        /// </summary>
        private async Task<(UserSession session, string refreshToken)> CreateSessionAsync(Guid userId, DateTime now)
        {
            var refreshTokenExpiryDays = configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays");
            var (refreshToken, refreshTokenHash) = await GenerateUniqueRefreshTokenPairAsync();

            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshTokenHash = refreshTokenHash,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddDays(refreshTokenExpiryDays),
                LastUsedAtUtc = now
            };

            context.UserSessions.Add(session);
            return (session, refreshToken);
        }

        /// <summary>
        /// Determines whether a session is still valid for refresh/logout operations.
        /// </summary>
        private static bool IsSessionActive(UserSession session, DateTime now)
        {
            return session.RevokedAtUtc == null && session.ExpiresAtUtc > now;
        }

        /// <summary>
        /// Marks a session as revoked once; repeated calls are no-ops.
        /// </summary>
        private static void RevokeSession(UserSession session, DateTime now, string reason)
        {
            if (session.RevokedAtUtc is not null)
            {
                return;
            }

            session.RevokedAtUtc = now;
            session.RevokeReason = reason;
        }

        /// <summary>
        /// Builds the auth response sent to clients after login/refresh.
        /// </summary>
        private TokenResponseDTO BuildTokenResponse(User user, DateTime refreshTokenExpiry, string refreshToken)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry
            };
        }

        /// <summary>
        /// Returns the configured max active sessions, with a safe minimum of 1.
        /// </summary>
        private int GetMaxActiveSessions()
        {
            var configured = configuration.GetValue<int?>("JwtSettings:MaxActiveSessions") ?? DefaultMaxActiveSessions;
            return Math.Max(1, configured);
        }

        /// <summary>
        /// Hashes refresh tokens with HMACSHA256 and a server-side pepper.
        /// </summary>
        private string HashRefreshToken(string refreshToken)
        {
            var pepper = configuration.GetValue<string>("JwtSettings:RefreshTokenPepper");
            if (string.IsNullOrWhiteSpace(pepper))
            {
                throw new InvalidOperationException("JwtSettings:RefreshTokenPepper is required.");
            }

            var key = Encoding.UTF8.GetBytes(pepper);
            var payload = Encoding.UTF8.GetBytes(refreshToken);
            using var hmac = new HMACSHA256(key);
            var hashBytes = hmac.ComputeHash(payload);
            return Convert.ToHexString(hashBytes);
        }

        /// <summary>
        /// Logs out only the current session, identified by userId + refresh token cookie.
        /// </summary>
        public async Task LogoutAsync(Guid userId, string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var refreshTokenHash = HashRefreshToken(refreshToken);
            var now = DateTime.UtcNow;
            var session = await context.UserSessions
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.RefreshTokenHash == refreshTokenHash &&
                    s.RevokedAtUtc == null &&
                    s.ExpiresAtUtc > now);

            if (session is null)
            {
                return;
            }

            RevokeSession(session, now, RevokeReasonUserLogout);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Revokes all active sessions for a user and returns the number of sessions revoked.
        /// </summary>
        public async Task<int> LogoutAllAsync(Guid userId)
        {
            var now = DateTime.UtcNow;
            var activeSessions = await context.UserSessions
                .Where(s => s.UserId == userId && s.RevokedAtUtc == null && s.ExpiresAtUtc > now)
                .ToListAsync();

            foreach (var session in activeSessions)
            {
                RevokeSession(session, now, RevokeReasonUserLogoutAll);
            }

            await context.SaveChangesAsync();
            return activeSessions.Count;
        }

        /// <summary>
        /// Creates a signed JWT access token with user identity and role claims.
        /// </summary>
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var accessTokenExpiryMinutes = configuration.GetValue<int>("JwtSettings:AccessTokenExpiryMinutes");

            var tokenDescriptor = new JwtSecurityToken
            (
                issuer: configuration.GetValue<string>("JwtSettings:Issuer"),
                audience: configuration.GetValue<string>("JwtSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
