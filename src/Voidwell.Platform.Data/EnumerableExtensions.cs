namespace Voidwell.Platform.Data;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ExceptBy<T, TKey>(this IEnumerable<T> items, IEnumerable<T> other, Func<T, TKey> getKey)
        where TKey : notnull
    {
        var otherKeys = other.Select(getKey).ToHashSet();

        return items.Where(item => !otherKeys.Contains(getKey(item)));
    }
}
