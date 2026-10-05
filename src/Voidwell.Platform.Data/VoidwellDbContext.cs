using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Voidwell.Platform.Data.Models;

namespace Voidwell.Platform.Data;

public class VoidwellDbContext : DbContext
{
    static VoidwellDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public VoidwellDbContext(DbContextOptions<VoidwellDbContext> options)
        : base(options)
    {
    }

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogPostTag> BlogPostTags => Set<BlogPostTag>();
    public DbSet<BlogPostTagMap> BlogPostTagMaps => Set<BlogPostTagMap>();
    public DbSet<CustomEvent> CustomEvents => Set<CustomEvent>();
    public DbSet<CustomEventTeam> CustomEventTeams => Set<CustomEventTeam>();

    internal static DbContextOptions<VoidwellDbContext> CreateDefaultDbContextOptions(DatabaseOptions options)
    {
        var builder = new DbContextOptionsBuilder<VoidwellDbContext>();

        Configure(builder, options);

        return builder.Options;
    }

    internal static void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
    {
        builder.UseNpgsql(options.ConnectionString, b =>
        {
            b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);

            if (options.CommandTimeout != null)
            {
                b.CommandTimeout(options.CommandTimeout);
            }
        })
        .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseSerialColumns();

        builder.ApplyConfigurationsFromAssembly(typeof(VoidwellDbContext).Assembly);
    }
}
