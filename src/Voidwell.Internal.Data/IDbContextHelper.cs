using static Voidwell.Internal.Data.DbContextHelper;

namespace Voidwell.Internal.Data
{
    public interface IDbContextHelper
    {
        DbContextFactory GetFactory();
    }
}
