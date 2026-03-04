using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shortly_API.Entities
{
    public class UserSession
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public string RefreshTokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAtUtc { get; set; }

        [Required]
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? LastUsedAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        [MaxLength(100)]
        public string? RevokeReason { get; set; }

        public Guid? ReplacedBySessionId { get; set; }

        [MaxLength(256)]
        public string? DeviceInfo { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(512)]
        public string? UserAgent { get; set; }
    }
}
