using Microsoft.EntityFrameworkCore;
using Shortly_API.Entities;

namespace Shortly_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>(entity =>
            {
                // Índice único en Email
                entity.HasIndex(e => e.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_Users_Email");

                entity.HasMany(e => e.Sessions)
                      .WithOne(s => s.User)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.CreatedAtUtc })
                      .HasDatabaseName("IX_UserSessions_UserId_CreatedAtUtc");

                entity.HasIndex(e => e.RefreshTokenHash)
                      .IsUnique()
                      .HasDatabaseName("UX_UserSessions_RefreshTokenHash");

                entity.HasIndex(e => e.ExpiresAtUtc)
                      .HasDatabaseName("IX_UserSessions_ExpiresAtUtc");
            });

            modelBuilder.Entity<ShortUrl>(entity =>
            {
                // Índice único en ShortCode, CRÍTICO para performance
                entity.HasIndex(e => e.ShortCode)
                      .IsUnique()
                      .HasDatabaseName("IX_ShortUrls_ShortCode");

                // Índice compuesto, optimiza búsquedas de URLs activas
                entity.HasIndex(e => new { e.IsActive, e.ShortCode })
                      .HasDatabaseName("IX_ShortUrls_IsActive_ShortCode");
            });
        }
    }
}
