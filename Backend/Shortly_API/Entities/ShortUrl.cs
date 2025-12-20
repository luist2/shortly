using Shortly_API.Utils;
using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Entities
{
    public class ShortUrl
    {
        [Key]
        public long Id { get; set; }

        [Required]
        // Máximo 10 caracteres
        [MaxLength(ShortCodeGenerator.MaxLength)]
        public string ShortCode { get; set; } = string.Empty;

        [Required]
        // 2048 caracteres cubre la mayoría de URLs
        [MaxLength(2048)]
        public string OriginalUrl { get; set; } = string.Empty;

        public Guid? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public int ClickCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime? LastAccessedAt { get; set; }

        // Relacion con la entidad User
        public virtual User? User { get; set; } = null!;
    }
}