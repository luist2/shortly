using System.ComponentModel.DataAnnotations;

namespace Shortly_API.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    }
}
