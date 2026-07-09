
namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemExtensionDataBuilderTests
    : CollectionTests<KeyValuePair<string, string>, ProblemExtensionDataBuilder, ProblemExtensionDataBuilder.Enumerator>
{
    private uint _nextId = 0;

    protected override ProblemExtensionDataBuilder Create(ReadOnlySpan<KeyValuePair<string, string>> items)
    {
        var builder = ProblemExtensionData.CreateBuilder();

        foreach (ref readonly var item in items)
        {
            builder.Add(item.Key, item.Value);
        }

        return builder;
    }

    protected override KeyValuePair<string, string> CreateDistinctItem()
    {
        var id = _nextId++;

        return new($"key-{id:D5}", $"value-{id:D5}");
    }

    protected override ProblemExtensionDataBuilder.Enumerator GetEnumerator(ProblemExtensionDataBuilder list)
        => list.GetEnumerator();

    [Theory]
    [MemberData(nameof(Counts))]
    public void Indexer_Returns_CorrectValue(int count)
    {
        var list = Create(count);

        var newItem = CreateDistinctItem();
        Should.Throw<KeyNotFoundException>(() => list[newItem.Key]);

        list.Add(newItem.Key, newItem.Value);
        list[newItem.Key].ShouldBe(newItem.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Indexer_CanModifyValue(int index)
    {
        var list = Create(5);
        var key = list.AsEnumerable().ElementAt(index).Key;

        list[key] = "new-value";
        list[key].ShouldBe("new-value");
    }

    [Fact]
    public void Indexer_CanAddValue()
    {
        var list = Create(5);
        list["new-key"] = "new-value";
        list.Count.ShouldBe(6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Remove_ByKey_WorksAsExpected(int index)
    {
        var list = Create(5);
        var key = list.AsEnumerable().ElementAt(index).Key;

        list.ShouldContainKey(key);
        list.Remove(key).ShouldBeTrue();
        list.ShouldNotContainKey(key);
        list.Remove(key).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(MergeCounts))]
    public void MergeWith_MergesItems_AndClearsOther(int leftCount, int rightCount)
    {
        var leftItems = CreateArray(leftCount);
        var rightItems = CreateArray(rightCount);
        var left = Create(leftItems);
        var right = Create(rightItems);

        left.MergeWith(ref right);

        left.Count.ShouldBe(leftCount + rightCount);
        right.Count.ShouldBe(0);

        foreach (var (key, value) in leftItems.Concat(rightItems))
        {
            left[key].ShouldBe(value);
        }
    }

    [Fact]
    public void MergeWith_UsesOtherValue_WhenKeysOverlap()
    {
        var left = ProblemExtensionData.CreateBuilder();
        var right = ProblemExtensionData.CreateBuilder();

        left.Add("shared", "left");
        left.Add("left-only", "left-value");
        right.Add("shared", "right");
        right.Add("right-only", "right-value");

        left.MergeWith(ref right);

        left.Count.ShouldBe(3);
        left["shared"].ShouldBe("right");
        left["left-only"].ShouldBe("left-value");
        left["right-only"].ShouldBe("right-value");
        right.Count.ShouldBe(0);
    }

    public static TheoryData<int, int> MergeCounts => new()
    {
        { 0, 0 },
        { 0, 1 },
        { 1, 0 },
        { 0, 9 },
        { 9, 0 },
        { 4, 4 },
        { 8, 1 },
        { 1, 8 },
        { 9, 1 },
        { 1, 9 },
        { 9, 9 },
        { 50, 1 },
        { 1, 50 },
        { 50, 50 },
    };
}
