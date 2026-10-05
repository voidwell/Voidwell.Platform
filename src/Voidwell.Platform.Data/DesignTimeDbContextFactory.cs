using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Voidwell.Platform.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VoidwellDbContext>
{
    public VoidwellDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("..", "Voidwell.Platform.Api", "appsettings.json"), optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(Path.Combine("..", "Voidwell.Platform.Api", "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = configuration.Get<DatabaseOptions>() ?? new DatabaseOptions();
        options.CommandTimeout ??= 180;

        return new VoidwellDbContext(VoidwellDbContext.CreateDefaultDbContextOptions(options));
    }
}
