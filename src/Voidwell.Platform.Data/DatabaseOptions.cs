namespace Voidwell.Platform.Data;

public class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int PoolSize { get; set; } = 5;
    public int? CommandTimeout { get; set; }
}
