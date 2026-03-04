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

            var newSession = await CreateSessionAsync(user.Id, now);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return BuildTokenResponse(user, newSession);
        }

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

        public async Task<TokenResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return null;
            }

            var now = DateTime.UtcNow;
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            // TODO: remove UserId from refresh request and resolve the session only from cookie token.
            var currentSession = await context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s =>
                    s.UserId == request.UserId &&
                    s.RefreshToken == request.RefreshToken);

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
                return BuildTokenResponse(user, currentSession);
            }

            RevokeSession(currentSession, now, RevokeReasonRotated);
            var rotatedSession = await CreateSessionAsync(user.Id, now);
            currentSession.ReplacedBySessionId = rotatedSession.Id;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return BuildTokenResponse(user, rotatedSession);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateUniqueRefreshTokenAsync()
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var token = GenerateRefreshToken();
                var exists = await context.UserSessions.AnyAsync(s => s.RefreshToken == token);
                if (!exists)
                {
                    return token;
                }
            }

            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private async Task<UserSession> CreateSessionAsync(Guid userId, DateTime now)
        {
            var refreshTokenExpiryDays = configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays");
            var refreshToken = await GenerateUniqueRefreshTokenAsync();
            var session = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RefreshToken = refreshToken,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddDays(refreshTokenExpiryDays),
                LastUsedAtUtc = now
            };

            context.UserSessions.Add(session);
            return session;
        }

        private static bool IsSessionActive(UserSession session, DateTime now)
        {
            return session.RevokedAtUtc == null && session.ExpiresAtUtc > now;
        }

        private static void RevokeSession(UserSession session, DateTime now, string reason)
        {
            if (session.RevokedAtUtc is not null)
            {
                return;
            }

            session.RevokedAtUtc = now;
            session.RevokeReason = reason;
        }

        private TokenResponseDTO BuildTokenResponse(User user, UserSession session)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = session.RefreshToken,
                RefreshTokenExpiry = session.ExpiresAtUtc
            };
        }

        private int GetMaxActiveSessions()
        {
            var configured = configuration.GetValue<int?>("JwtSettings:MaxActiveSessions") ?? DefaultMaxActiveSessions;
            return Math.Max(1, configured);
        }

        public async Task LogoutAsync(Guid userId, string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }

            var now = DateTime.UtcNow;
            var session = await context.UserSessions
                .FirstOrDefaultAsync(s =>
                    s.UserId == userId &&
                    s.RefreshToken == refreshToken &&
                    s.RevokedAtUtc == null &&
                    s.ExpiresAtUtc > now);

            if (session is null)
            {
                return;
            }

            RevokeSession(session, now, RevokeReasonUserLogout);

            await context.SaveChangesAsync();
        }

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
