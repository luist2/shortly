using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Models
{
    public class RefreshTokenRequestDTO
    {
        [Required]
        public Guid UserId { get; set; }

        public string? RefreshToken { get; set; }
    }
}
