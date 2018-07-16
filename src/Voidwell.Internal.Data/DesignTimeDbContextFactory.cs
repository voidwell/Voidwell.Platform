using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Voidwell.Internal.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VoidwellDbContext>
    {
        public VoidwellDbContext CreateDbContext(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("devsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<VoidwellDbContext>();

            var connectionString = configuration.GetValue<string>("ConnectionString");

            builder.UseNpgsql(connectionString, o =>
            {
                o.CommandTimeout(180);
            });

            return new VoidwellDbContext(builder.Options);
        }
    }
}
