namespace Shortly_API.Models
{
    public class RefreshTokenRequestDTO
    {
        public Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}
