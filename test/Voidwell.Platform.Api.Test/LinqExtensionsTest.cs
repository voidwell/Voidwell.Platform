using FluentAssertions;
using Xunit;

namespace Voidwell.Platform.Api.Test;

public class LinqExtensionsTest
{
    private sealed record Item(int Id, string Name);

    private static readonly Item[] _items =
    [
        new(1, "b"),
        new(2, "a"),
        new(3, "b"),
    ];

    [Fact]
    public void OrderBy_SortsAscendingByPropertyName()
    {
        var result = _items.AsQueryable().OrderBy("Name", SortDirection.Ascending);

        result.Select(a => a.Name).Should().Equal("a", "b", "b");
    }

    [Fact]
    public void OrderBy_SortsDescendingByPropertyName()
    {
        var result = _items.AsQueryable().OrderBy("Id", SortDirection.Descending);

        result.Select(a => a.Id).Should().Equal(3, 2, 1);
    }

    [Fact]
    public void ThenBy_AppliesSecondarySort()
    {
        var result = _items.AsQueryable()
            .OrderBy("Name", SortDirection.Ascending)
            .ThenBy("Id", SortDirection.Descending);

        result.Select(a => a.Id).Should().Equal(2, 3, 1);
    }

    [Fact]
    public void OrderBy_ReturnsUnsortedQuery_WhenPropertyDoesNotExist()
    {
        var result = _items.AsQueryable().OrderBy("Missing", SortDirection.Ascending);

        result.Should().Equal(_items);
    }
}
