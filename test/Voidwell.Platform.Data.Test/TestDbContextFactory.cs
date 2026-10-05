using Microsoft.EntityFrameworkCore;

namespace Voidwell.Platform.Data.Test;

internal sealed class TestDbContextFactory : IDbContextFactory<VoidwellDbContext>
{
    private readonly DbContextOptions<VoidwellDbContext> _options;

    public TestDbContextFactory()
    {
        _options = new DbContextOptionsBuilder<VoidwellDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    public VoidwellDbContext CreateDbContext()
    {
        return new VoidwellDbContext(_options);
    }
}
