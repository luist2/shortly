using Shortly_API.Entities;
using Shortly_API.Models;

namespace Shortly_API.Services
{
    public interface IAuthService
    {
        Task<UserResponseDTO?> RegisterAsync(UserDTO request);
        Task<TokenResponseDTO?> LoginAsync(UserDTO request);
        Task<TokenResponseDTO> RefreshTokensAsync(RefreshTokenRequestDTO request);
    }
}
