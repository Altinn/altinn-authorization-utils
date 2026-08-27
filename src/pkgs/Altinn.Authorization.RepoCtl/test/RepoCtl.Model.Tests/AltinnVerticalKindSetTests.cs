using System.Collections.Immutable;

namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class AltinnVerticalKindSetTests
{
    [Fact]
    public void Default_IsEmptySet()
    {
        var set = AltinnVerticalKindSet.Empty;

        set.Count.ShouldBe(0);
        set.ShouldBeEmpty();
        set.ToString().ShouldBe("none");
    }

    [Fact]
    public void CollectionExpression_CreatesSetWithDistinctKinds()
    {
        AltinnVerticalKindSet set = [
            AltinnVerticalKind.Application,
            AltinnVerticalKind.Library,
            AltinnVerticalKind.Application,
        ];

        set.Count.ShouldBe(2);
        ShouldHaveItems(set, AltinnVerticalKind.Application, AltinnVerticalKind.Library);
    }

    [Fact]
    public void Add_AddsDistinctKinds()
    {
        var set = AltinnVerticalKindSet.Empty
            .Add(AltinnVerticalKind.Library)
            .Add(AltinnVerticalKind.Application)
            .Add(AltinnVerticalKind.Library);

        set.Count.ShouldBe(2);
        set.Contains(AltinnVerticalKind.Application).ShouldBeTrue();
        set.Contains(AltinnVerticalKind.Library).ShouldBeTrue();
        ShouldHaveItems(set, AltinnVerticalKind.Application, AltinnVerticalKind.Library);
    }

    [Fact]
    public void Contains_WhenItemIsDefault_ReturnsFalse()
    {
        var set = AltinnVerticalKindSet.Application;

        set.Contains(default).ShouldBeFalse();
    }

    [Fact]
    public void TryGetValue_WhenItemIsDefault_ReturnsFalse()
    {
        IImmutableSet<AltinnVerticalKind> set = AltinnVerticalKindSet.Application;

        var success = set.TryGetValue(default, out var actual);

        success.ShouldBeFalse();
        actual.ShouldBe(default);
    }

    [Fact]
    public void Remove_RemovesKind()
    {
        var set = AltinnVerticalKindSet.Application
            .Add(AltinnVerticalKind.Library)
            .Remove(AltinnVerticalKind.Application);

        ShouldHaveItems(set, AltinnVerticalKind.Library);
    }

    [Fact]
    public void SetOperations_ReturnExpectedKinds()
    {
        var left = AltinnVerticalKindSet.Application
            .Add(AltinnVerticalKind.Library)
            .Add(AltinnVerticalKind.Package);
        var right = AltinnVerticalKindSet.Library
            .Add(AltinnVerticalKind.Tool);

        ShouldHaveItems(left.Intersect(right), AltinnVerticalKind.Library);
        ShouldHaveItems(left.Except(right), AltinnVerticalKind.Application, AltinnVerticalKind.Package);
        ShouldHaveItems(left.SymmetricExcept(right), AltinnVerticalKind.Application, AltinnVerticalKind.Package, AltinnVerticalKind.Tool);
        ShouldHaveItems(left.Union(right), AltinnVerticalKind.Application, AltinnVerticalKind.Package, AltinnVerticalKind.Tool, AltinnVerticalKind.Library);
    }

    [Fact]
    public void RelationshipChecks_CompareSets()
    {
        var subset = AltinnVerticalKindSet.Application.Add(AltinnVerticalKind.Library);
        var superset = subset.Add(AltinnVerticalKind.Package);

        subset.IsSubsetOf(superset).ShouldBeTrue();
        subset.IsProperSubsetOf(superset).ShouldBeTrue();
        superset.IsSupersetOf(subset).ShouldBeTrue();
        superset.IsProperSupersetOf(subset).ShouldBeTrue();
        subset.Overlaps(AltinnVerticalKindSet.Library).ShouldBeTrue();
        subset.SetEquals(AltinnVerticalKindSet.Library.Add(AltinnVerticalKind.Application)).ShouldBeTrue();
    }

    [Fact]
    public void IReadOnlySetMethods_AcceptEnumerable()
    {
        IReadOnlySet<AltinnVerticalKind> set = AltinnVerticalKindSet.Application.Add(AltinnVerticalKind.Library);

        set.IsSubsetOf([AltinnVerticalKind.Application, AltinnVerticalKind.Library, AltinnVerticalKind.Package]).ShouldBeTrue();
        set.IsSupersetOf([AltinnVerticalKind.Library]).ShouldBeTrue();
        set.Overlaps([AltinnVerticalKind.Package, AltinnVerticalKind.Library]).ShouldBeTrue();
        set.SetEquals([AltinnVerticalKind.Library, AltinnVerticalKind.Application]).ShouldBeTrue();
    }

    [Fact]
    public void IImmutableSetMethods_ReturnUpdatedSets()
    {
        IImmutableSet<AltinnVerticalKind> set = AltinnVerticalKindSet.Application;

        set = set.Add(AltinnVerticalKind.Library);
        ShouldHaveItems(set, AltinnVerticalKind.Application, AltinnVerticalKind.Library);

        set = set.Remove(AltinnVerticalKind.Application);
        ShouldHaveItems(set, AltinnVerticalKind.Library);

        set.Clear().ShouldBeEmpty();
    }

    [Fact]
    public void ToString_WithMultipleKinds_FormatsInKindOrder()
    {
        var set = AltinnVerticalKindSet.Tool
            .Add(AltinnVerticalKind.Package)
            .Add(AltinnVerticalKind.Application)
            .Add(AltinnVerticalKind.Library);

        set.ToString().ShouldBe("app, pkg, tool, lib");
    }

    [Fact]
    public void TryFormat_WithMultipleKinds_WritesText()
    {
        var set = AltinnVerticalKindSet.Application
            .Add(AltinnVerticalKind.Library)
            .Add(AltinnVerticalKind.Package);
        Span<char> destination = stackalloc char[32];

        var success = set.TryFormat(destination, out var charsWritten, default, provider: null);

        success.ShouldBeTrue();
        destination[..charsWritten].ToString().ShouldBe("app, pkg, lib");
    }

    [Fact]
    public void TryFormat_WhenDestinationIsTooSmall_ReturnsFalse()
    {
        var set = AltinnVerticalKindSet.Application.Add(AltinnVerticalKind.Library);
        Span<char> destination = stackalloc char[4];

        var success = set.TryFormat(destination, out var charsWritten, default, provider: null);

        success.ShouldBeFalse();
        charsWritten.ShouldBe(3);
    }

    private static void ShouldHaveItems(IEnumerable<AltinnVerticalKind> actual, params AltinnVerticalKind[] expected)
    {
        actual.ToArray().ShouldBe(expected);
    }
}
