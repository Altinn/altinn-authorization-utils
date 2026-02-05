using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public abstract class CollectionTests<TItem, TList, TEnumerator>
    where TList : ICollection<TItem>
    where TEnumerator : IEnumerator<TItem>
{
    protected abstract TList Create(ReadOnlySpan<TItem> items);
    protected abstract TItem CreateDistinctItem();
    protected abstract TEnumerator GetEnumerator(TList list);

    protected TItem[] CreateArray(int count)
    {
        var items = new TItem[count];
        for (var i = 0; i < count; i++)
        {
            items[i] = CreateDistinctItem();
        }

        return items;
    }

    protected TList Create(int count)
        => Create(CreateArray(count));

    [Fact]
    public void Count_Empty_ShouldBeZero()
    {
        var list = Create([]);

        list.Count.ShouldBe(0);
    }

    [Theory]
    [MemberData(nameof(Counts))]
    public void Count_ReturnsCount(int count)
    {
        var list = Create(count);

        list.Count.ShouldBe(count);
    }

    [Fact]
    public void IsNotReadOnly()
    {
        var list = Create([]);

        list.IsReadOnly.ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(Counts))]
    public void Enumerates_AllItems(int count)
    {
        var source = CreateArray(count);
        var list = Create(source);

        var sourceEnumerator = source.GetEnumerator();
        var listEnumerator = GetEnumerator(list);

        while (sourceEnumerator.MoveNext())
        {
            listEnumerator.MoveNext().ShouldBeTrue();
            listEnumerator.Current.ShouldBe(sourceEnumerator.Current);
        }
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var list = Create(5);

        list.Clear();
        list.Count.ShouldBe(0);
        list.GetEnumerator().MoveNext().ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(Counts))]
    public void Contains_WorksAsExpected(int count)
    {
        var list = Create(count);
        var notInList = CreateDistinctItem();

        list.Contains(notInList).ShouldBeFalse();
        list.Add(notInList);
        list.Contains(notInList).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(Counts))]
    public void CopyTo_WorksAsExpected(int count)
    {
        var source = CreateArray(count);
        var list = Create(source);
        var array = new TItem[count + 2];

        list.CopyTo(array, 1);

        array.AsSpan(1, count).SequenceEqual(source).ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Remove_WorksAsExpected(int removeIndex)
    {
        var source = CreateArray(5);
        var list = Create(source);

        var toRemove = source[removeIndex];
        var rest = source.Where((_, i) => i != removeIndex).ToArray();

        list.ShouldContain(toRemove);
        list.Remove(toRemove).ShouldBe(true);
        list.ShouldNotContain(toRemove);
        list.Remove(toRemove).ShouldBeFalse();

        foreach (var item in rest)
        {
            list.ShouldContain(item);
        }
    }

    public static TheoryData<int> Counts => new([
        0,
        1,
        5,
        50,
        500,
    ]);
}
