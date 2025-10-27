using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Models
{
    public class RefreshTokenRequestDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public required string RefreshToken { get; set; }
    }
}
