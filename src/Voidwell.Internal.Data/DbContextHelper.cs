using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Voidwell.Internal.Data
{
    public class DbContextHelper : IDbContextHelper
    {
        private readonly DbContextOptions<VoidwellDbContext> _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public DbContextHelper(DbContextOptions<VoidwellDbContext> options, IServiceScopeFactory scopeFactory)
        {
            _options = options;
            _scopeFactory = scopeFactory;
        }

        public DbContextFactory GetFactory()
        {
            return new DbContextFactory(_scopeFactory);
        }

        public class DbContextFactory : IDisposable
        {
            private readonly IServiceScope _scope;
            private readonly VoidwellDbContext _dbContext;

            public DbContextFactory(IServiceScopeFactory scopeFactory)
            {
                _scope = scopeFactory.CreateScope();
                _dbContext = _scope.ServiceProvider.GetRequiredService<VoidwellDbContext>();
            }

            public VoidwellDbContext GetDbContext()
            {
                return _dbContext;
            }

            public void Dispose()
            {
                _scope.Dispose();
            }
        }
    }
}
