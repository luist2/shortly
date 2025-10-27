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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>(entity =>
            {
                // Índice único en Email
                entity.HasIndex(e => e.Email)
                      .IsUnique()
                      .HasDatabaseName("IX_Users_Email");
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
