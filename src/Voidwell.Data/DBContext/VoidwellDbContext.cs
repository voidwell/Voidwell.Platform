using Microsoft.EntityFrameworkCore;
using Voidwell.Data.Models;

namespace Voidwell.Data.DBContext
{
    public class VoidwellDbContext : DbContext
    {
        private readonly DatabaseOptions _options;

        public VoidwellDbContext(DatabaseOptions options)
        {
            _options = options;
        }

        public DbSet<DbBlogPost> BlogPosts { get; set; }
        public DbSet<DbCustomEvent> CustomEvents { get; set; }
        public DbSet<DbCustomEventTeam> CustomEventTeams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_options.DBConnectionString, b => b.MigrationsAssembly("Voidwell.Data"));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<DbCustomEventTeam>()
                .HasKey(a => new { a.EventId, a.TeamId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
