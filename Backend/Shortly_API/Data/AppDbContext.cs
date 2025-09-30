using Microsoft.EntityFrameworkCore;
using Shortly_API.Entities;

namespace Shortly_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){
        }

        public DbSet<User> Users => Set<User>();
    }
}
