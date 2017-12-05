using Microsoft.EntityFrameworkCore;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data
{
    public class VoidwellDbContext : DbContext
    {
        public VoidwellDbContext(DbContextOptions<VoidwellDbContext> options)
            : base(options)
        {
        }

        public DbSet<DbBlogPost> BlogPosts { get; set; }
        public DbSet<DbCustomEvent> CustomEvents { get; set; }
        public DbSet<DbCustomEventTeam> CustomEventTeams { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DbCustomEventTeam>()
                .HasKey(a => new { a.EventId, a.TeamId });
        }
    }
}
