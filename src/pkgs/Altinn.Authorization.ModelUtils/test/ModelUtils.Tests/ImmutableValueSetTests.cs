using System.Collections;
using System.Collections.Immutable;

namespace Altinn.Authorization.ModelUtils.Tests;

public class ImmutableValueSetTests
{
    [Fact]
    public void Empty_IsEmptySet()
    {
        var set = ImmutableValueSet<int>.Empty;

        set.IsEmpty.ShouldBeTrue();
        set.Count.ShouldBe(0);
        set.ShouldBeEmpty();
        set.Comparer.ShouldBe(Comparer<int>.Default);
    }

    [Fact]
    public void Create_WithoutItems_ReturnsEmptySet()
    {
        var set = ImmutableValueSet.Create<int>();

        set.ShouldBeEmpty();
        set.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Create_WithItems_RemovesDuplicatesAndSortsByComparer()
    {
        var set = ImmutableValueSet.Create(3, 1, 2, 3, 1);

        ShouldHaveItems(set, 1, 2, 3);
        set.Count.ShouldBe(3);
        set.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithComparer_UsesComparerForOrderingAndUniqueness()
    {
        var comparer = Comparer<int>.Create(static (left, right) => right.CompareTo(left));

        var set = ImmutableValueSet.Create(comparer, 1, 3, 2, 3, 1);

        ShouldHaveItems(set, 3, 2, 1);
        set.Comparer.ShouldBeSameAs(comparer);
    }

    [Fact]
    public void Create_WithCaseInsensitiveComparer_UsesComparerForValueEquality()
    {
        var set = ImmutableValueSet.Create(StringComparer.OrdinalIgnoreCase, "alpha", "ALPHA", "beta");

        ShouldHaveItems(set, "alpha", "beta");
        set.Contains("ALPHA").ShouldBeTrue();
        set.Contains("Beta").ShouldBeTrue();
    }

    [Fact]
    public void CollectionExpression_RemovesDuplicatesAndSorts()
    {
        ImmutableValueSet<int> set = [4, 2, 4, 1, 3, 2];

        ShouldHaveItems(set, 1, 2, 3, 4);
    }

    [Fact]
    public void EnumerableExtension_CreatesImmutableValueSet()
    {
        IEnumerable<int> values = [3, 1, 3, 2];

        var set = values.ToImmutableValueSet();

        ShouldHaveItems(set, 1, 2, 3);
    }

    [Fact]
    public void EnumerableExtension_WithComparer_UsesComparer()
    {
        IEnumerable<string> values = ["b", "A", "a"];

        var set = values.ToImmutableValueSet(StringComparer.OrdinalIgnoreCase);

        ShouldHaveItems(set, "A", "b");
        set.Contains("B").ShouldBeTrue();
    }

    [Fact]
    public void IReadOnlyListMembers_ExposeSortedContents()
    {
        IReadOnlyList<int> set = ImmutableValueSet.Create(3, 1, 2);

        set.Count.ShouldBe(3);
        set[0].ShouldBe(1);
        set[1].ShouldBe(2);
        set[2].ShouldBe(3);
    }

    [Fact]
    public void Indexer_WhenIndexIsOutOfRange_Throws()
    {
        var set = ImmutableValueSet.Create(1, 2, 3);

        Should.Throw<IndexOutOfRangeException>(() => set[-1]);
        Should.Throw<IndexOutOfRangeException>(() => set[3]);
    }

    [Fact]
    public void TryGetValue_WhenEquivalentValueExists_ReturnsStoredValue()
    {
        var set = ImmutableValueSet.Create(StringComparer.OrdinalIgnoreCase, "Alpha");

        var found = set.TryGetValue("alpha", out var actual);

        found.ShouldBeTrue();
        actual.ShouldBe("Alpha");
    }

    [Fact]
    public void TryGetValue_WhenEquivalentValueDoesNotExist_ReturnsFalse()
    {
        var set = ImmutableValueSet.Create(StringComparer.OrdinalIgnoreCase, "Alpha");

        var found = set.TryGetValue("beta", out var actual);

        found.ShouldBeFalse();
        actual.ShouldBeNull();
    }

    [Fact]
    public void Clear_ReturnsEmptySetWithSameComparer()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var set = ImmutableValueSet.Create(comparer, "a", "b");

        var cleared = set.Clear();

        cleared.ShouldBeEmpty();
        cleared.Comparer.ShouldBeSameAs(comparer);
    }

    [Fact]
    public void ToBuilder_StartsWithSameContentsAndComparer()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var set = ImmutableValueSet.Create(comparer, "b", "a");

        var builder = set.ToBuilder();

        ShouldHaveItems(builder, "a", "b");
        builder.Comparer.ShouldBeSameAs(comparer);
    }

    [Fact]
    public void ToBuilder_MutationsDoNotChangeSourceSet()
    {
        var set = ImmutableValueSet.Create(1, 2, 3);
        var builder = set.ToBuilder();

        builder.Add(4).ShouldBeTrue();
        builder.Remove(1).ShouldBeTrue();

        ShouldHaveItems(set, 1, 2, 3);
        ShouldHaveItems(builder, 2, 3, 4);
    }

    [Fact]
    public void ToImmutable_ReturnsSnapshotThatDoesNotChangeWithBuilder()
    {
        var builder = ImmutableValueSet.CreateBuilder<int>();
        builder.UnionWith([3, 1, 2]);

        var snapshot = builder.ToImmutable();
        builder.Add(4).ShouldBeTrue();
        builder.Remove(1).ShouldBeTrue();

        ShouldHaveItems(snapshot, 1, 2, 3);
        ShouldHaveItems(builder, 2, 3, 4);
    }

    [Fact]
    public void BuilderExtension_CreatesImmutableValueSet()
    {
        var builder = ImmutableValueSet.CreateBuilder<int>();
        builder.UnionWith([2, 1, 2]);

        var set = builder.ToImmutableValueSet();

        ShouldHaveItems(set, 1, 2);
    }

    [Fact]
    public void Builder_Add_ReturnsWhetherSetChanged()
    {
        var builder = ImmutableValueSet.CreateBuilder<int>();

        builder.Add(2).ShouldBeTrue();
        builder.Add(1).ShouldBeTrue();
        builder.Add(2).ShouldBeFalse();

        ShouldHaveItems(builder, 1, 2);
    }

    [Fact]
    public void Builder_ICollectionAdd_IgnoresDuplicate()
    {
        var builder = ImmutableValueSet.CreateBuilder<int>();
        ICollection<int> collection = builder;

        collection.Add(2);
        collection.Add(1);
        collection.Add(2);

        ShouldHaveItems(builder, 1, 2);
    }

    [Fact]
    public void Builder_Remove_ReturnsWhetherSetChanged()
    {
        var builder = ImmutableValueSet.Create(1, 2, 3).ToBuilder();

        builder.Remove(2).ShouldBeTrue();
        builder.Remove(4).ShouldBeFalse();

        ShouldHaveItems(builder, 1, 3);
    }

    [Fact]
    public void Builder_Clear_RemovesAllItems()
    {
        var builder = ImmutableValueSet.Create(1, 2, 3).ToBuilder();

        builder.Clear();

        builder.ShouldBeEmpty();
        builder.Count.ShouldBe(0);
    }

    [Fact]
    public void Builder_IsMutableCollection()
    {
        ICollection<int> builder = ImmutableValueSet.CreateBuilder<int>();

        builder.IsReadOnly.ShouldBeFalse();
    }

    [Fact]
    public void Builder_CopyTo_CopiesCurrentContentsToRequestedOffset()
    {
        ICollection<int> builder = ImmutableValueSet.Create(3, 1, 2).ToBuilder();
        int[] target = [0, 0, 0, 0, 0];

        builder.CopyTo(target, 1);

        target.ShouldBe([0, 1, 2, 3, 0]);
    }

    [Fact]
    public void Builder_ComparerSetter_RebuildsUsingNewComparer()
    {
        var builder = ImmutableValueSet.Create("a", "A", "b").ToBuilder();

        builder.Comparer = StringComparer.OrdinalIgnoreCase;

        ShouldHaveItems(builder, "a", "b");
        builder.Contains("A").ShouldBeTrue();
    }

    [Fact]
    public void Builder_ComparerSetter_WhenNull_Throws()
    {
        var builder = ImmutableValueSet.CreateBuilder<string>();

        Should.Throw<ArgumentNullException>(() => builder.Comparer = null!);
    }

    [Fact]
    public void Builder_UnionWith_AddsMissingItemsOnly()
    {
        var builder = ImmutableValueSet.Create(1, 3).ToBuilder();

        builder.UnionWith([3, 2, 2, 1]);

        ShouldHaveItems(builder, 1, 2, 3);
    }

    [Fact]
    public void Builder_ExceptWith_RemovesMatchingItems()
    {
        var builder = ImmutableValueSet.Create(1, 2, 3, 4).ToBuilder();

        builder.ExceptWith([2, 4, 4, 5]);

        ShouldHaveItems(builder, 1, 3);
    }

    [Fact]
    public void Builder_IntersectWith_KeepsOnlyItemsPresentInOtherAsASet()
    {
        var builder = ImmutableValueSet.Create(1, 2, 3, 4).ToBuilder();

        builder.IntersectWith([2, 2, 4, 5]);

        ShouldHaveItems(builder, 2, 4);
    }

    [Fact]
    public void Builder_SymmetricExceptWith_TreatsOtherAsASet()
    {
        var builder = ImmutableValueSet.Create(1, 2, 3).ToBuilder();

        builder.SymmetricExceptWith([2, 2, 4, 4, 5]);

        ShouldHaveItems(builder, 1, 3, 4, 5);
    }

    [Fact]
    public void Builder_SetRelationshipMembers_FollowISetContract()
    {
        ISet<int> set = ImmutableValueSet.Create(2, 3).ToBuilder();

        set.IsSubsetOf([1, 2, 3]).ShouldBeTrue();
        set.IsProperSubsetOf([1, 2, 3]).ShouldBeTrue();
        set.IsSupersetOf([2]).ShouldBeTrue();
        set.IsProperSupersetOf([2]).ShouldBeTrue();
        set.Overlaps([3, 4]).ShouldBeTrue();
        set.SetEquals([3, 2, 2]).ShouldBeTrue();
    }

    [Fact]
    public void Builder_Enumerator_WhenBuilderIsMutatedAfterEnumerationStarts_Throws()
    {
        var builder = ImmutableValueSet.Create(1, 2, 3).ToBuilder();
        using var enumerator = builder.GetEnumerator();

        enumerator.MoveNext().ShouldBeTrue();
        builder.Add(4);

        Should.Throw<InvalidOperationException>(() => enumerator.MoveNext());
    }

    [Fact]
    public void IImmutableSet_Add_ReturnsNewSetAndLeavesOriginalUnchanged()
    {
        var original = ImmutableValueSet.Create(1, 3);
        IImmutableSet<int> set = original;

        var result = set.Add(2);

        ShouldHaveItems(original, 1, 3);
        ShouldHaveItems(result, 1, 2, 3);
    }

    [Fact]
    public void IImmutableSet_AddDuplicate_ReturnsEquivalentSet()
    {
        IImmutableSet<int> set = ImmutableValueSet.Create(1, 2);

        var result = set.Add(2);

        ShouldHaveItems(result, 1, 2);
    }

    [Fact]
    public void IImmutableSet_Remove_ReturnsNewSetAndLeavesOriginalUnchanged()
    {
        var original = ImmutableValueSet.Create(1, 2, 3);
        IImmutableSet<int> set = original;

        var result = set.Remove(2);

        ShouldHaveItems(original, 1, 2, 3);
        ShouldHaveItems(result, 1, 3);
    }

    [Fact]
    public void IImmutableSet_Union_ReturnsSetUnion()
    {
        IImmutableSet<int> set = ImmutableValueSet.Create(1, 3);

        var result = set.Union([3, 2, 2]);

        ShouldHaveItems(result, 1, 2, 3);
    }

    [Fact]
    public void IImmutableSet_Intersect_ReturnsSetIntersection()
    {
        IImmutableSet<int> set = ImmutableValueSet.Create(1, 2, 3, 4);

        var result = set.Intersect([2, 2, 4, 5]);

        ShouldHaveItems(result, 2, 4);
    }

    [Fact]
    public void IImmutableSet_Except_ReturnsSetDifference()
    {
        IImmutableSet<int> set = ImmutableValueSet.Create(1, 2, 3, 4);

        var result = set.Except([2, 2, 4, 5]);

        ShouldHaveItems(result, 1, 3);
    }

    [Fact]
    public void IImmutableSet_SymmetricExcept_ReturnsSymmetricSetDifference()
    {
        IImmutableSet<int> set = ImmutableValueSet.Create(1, 2, 3);

        var result = set.SymmetricExcept([2, 2, 4, 4, 5]);

        ShouldHaveItems(result, 1, 3, 4, 5);
    }

    [Fact]
    public void IImmutableSet_Clear_ReturnsEmptySetWithSameComparer()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        IImmutableSet<string> set = ImmutableValueSet.Create(comparer, "a", "b");

        var result = set.Clear().ShouldBeOfType<ImmutableValueSet<string>>();

        result.ShouldBeEmpty();
        result.Comparer.ShouldBeSameAs(comparer);
    }

    [Fact]
    public void IImmutableSet_SetRelationshipMembers_FollowIImmutableSetContract()
    {
        IImmutableSet<int> set = ImmutableValueSet.Create(2, 3);

        set.IsSubsetOf([1, 2, 3]).ShouldBeTrue();
        set.IsProperSubsetOf([1, 2, 3]).ShouldBeTrue();
        set.IsSupersetOf([2]).ShouldBeTrue();
        set.IsProperSupersetOf([2]).ShouldBeTrue();
        set.Overlaps([3, 4]).ShouldBeTrue();
        set.SetEquals([3, 2, 2]).ShouldBeTrue();
    }

    [Fact]
    public void IReadOnlySet_SetRelationshipMembers_FollowIReadOnlySetContract()
    {
        IReadOnlySet<int> set = ImmutableValueSet.Create(2, 3);

        set.IsSubsetOf([1, 2, 3]).ShouldBeTrue();
        set.IsProperSubsetOf([1, 2, 3]).ShouldBeTrue();
        set.IsSupersetOf([2]).ShouldBeTrue();
        set.IsProperSupersetOf([2]).ShouldBeTrue();
        set.Overlaps([3, 4]).ShouldBeTrue();
        set.SetEquals([3, 2, 2]).ShouldBeTrue();
    }

    [Fact]
    public void PublicSetEquals_UsesSetSemantics()
    {
        var left = ImmutableValueSet.Create(1, 2, 3);
        var right = ImmutableValueSet.Create(3, 2, 1);

        left.SetEquals(right).ShouldBeTrue();
    }

    [Fact]
    public void Equality_WithSameComparerAndItems_IsTrue()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var left = ImmutableValueSet.Create(comparer, "a", "b");
        var right = ImmutableValueSet.Create(comparer, "B", "A");

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
    }

    [Fact]
    public void Equality_WithDifferentComparerInstances_IsFalse()
    {
        var left = ImmutableValueSet.Create(StringComparer.OrdinalIgnoreCase, "a", "b");
        var right = ImmutableValueSet.Create(StringComparer.InvariantCultureIgnoreCase, "a", "b");

        left.Equals(right).ShouldBeFalse();
        (left == right).ShouldBeFalse();
        (left != right).ShouldBeTrue();
    }

    [Fact]
    public void Equality_WithEqualSets_ProducesEqualHashCodes()
    {
        var left = ImmutableValueSet.Create(1, 2, 3);
        var right = ImmutableValueSet.Create(3, 2, 1);

        left.Equals(right).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void Equality_WithNull_IsFalse()
    {
        var set = ImmutableValueSet.Create(1, 2, 3);

        set.Equals(null).ShouldBeFalse();
        (set == null).ShouldBeFalse();
        (null == set).ShouldBeFalse();
        (set != null).ShouldBeTrue();
        (null != set).ShouldBeTrue();
    }

    [Fact]
    public void NonGenericEnumeration_ReturnsSortedContents()
    {
        IEnumerable enumerable = ImmutableValueSet.Create(3, 1, 2);

        ShouldHaveItems(enumerable.Cast<int>(), 1, 2, 3);
    }

    private static void ShouldHaveItems<T>(IEnumerable<T> actual, params T[] expected)
        => actual.ToArray().ShouldBe(expected);
}
