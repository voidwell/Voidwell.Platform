using FluentAssertions;
using Xunit;

namespace Voidwell.Platform.Data.Test;

public class EnumerableExtensionsTest
{
    private sealed record Item(string Key, int Value);

    [Fact]
    public void ExceptBy_ReturnsItemsWhoseKeyIsNotInOther()
    {
        Item[] items = [new("a", 1), new("b", 2), new("c", 3)];
        Item[] other = [new("b", 99)];

        var result = items.ExceptBy(other, a => a.Key);

        result.Select(a => a.Key).Should().Equal("a", "c");
    }

    [Fact]
    public void ExceptBy_ReturnsAllItems_WhenOtherIsEmpty()
    {
        Item[] items = [new("a", 1), new("b", 2)];

        var result = items.ExceptBy([], a => a.Key);

        result.Should().Equal(items);
    }

    [Fact]
    public void ExceptBy_ReturnsNothing_WhenAllKeysMatch()
    {
        Item[] items = [new("a", 1)];

        var result = items.ExceptBy([new Item("a", 2)], a => a.Key);

        result.Should().BeEmpty();
    }
}
