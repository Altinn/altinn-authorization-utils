using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class MultipleProblemBuilderTests
    : CollectionTests<ProblemInstance, MultipleProblemBuilder, CollectionBuilderEnumerator<ProblemInstance>>
{
    private readonly ProblemDescriptorFactory _factory = ProblemDescriptorFactory.New("TEST");
    private uint _nextId = 0;

    protected override MultipleProblemBuilder Create(ReadOnlySpan<ProblemInstance> items)
    {
        var builder = MultipleProblemInstance.CreateBuilder();

        foreach (ref readonly var item in items)
        {
            builder.Add(item);
        }

        return builder;
    }

    protected override ProblemInstance CreateDistinctItem()
    {
        var id = _nextId++;

        return _factory.Create(id, HttpStatusCode.InternalServerError, $"Error {id}").Create();
    }

    protected override CollectionBuilderEnumerator<ProblemInstance> GetEnumerator(MultipleProblemBuilder list)
        => list.GetEnumerator();

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = MultipleProblemInstance.CreateBuilder();

        errors.TryBuild(out var instance).ShouldBeFalse();
        errors.TryToProblemDetails(out var details).ShouldBeFalse();
        errors.TryToActionResult(out var result).ShouldBeFalse();

        instance.ShouldBeNull();
        details.ShouldBeNull();
        result.ShouldBeNull();
    }

    [Fact]
    public void Single_NoExtensions_TryTo_Returns_Inner()
    {
        var errors = MultipleProblemInstance.CreateBuilder();

        errors.Add(TestErrors.InternalServerError);

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();

        instance.ErrorCode.ShouldBe(TestErrors.InternalServerError.ErrorCode);
        details.ErrorCode.ShouldBe(TestErrors.InternalServerError.ErrorCode);
    }

    [Fact]
    public void Single_WithExtensions_TryTo_Returns_Multiple()
    {
        var errors = MultipleProblemInstance.CreateBuilder();

        errors.Add(TestErrors.InternalServerError);
        errors.AddExtension("foo", "bar");

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();

        var multipleInstance = instance.ShouldBeOfType<MultipleProblemInstance>();
        multipleInstance.Problems.Length.ShouldBe(1);

        instance.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
        details.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
    }

    [Fact]
    public void Multiple_TryTo_Returns_Multiple()
    {
        var errors = MultipleProblemInstance.CreateBuilder();

        errors.Add(TestErrors.InternalServerError);
        errors.Add(TestErrors.NotImplemented, [new("foo", "bar")]);

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();

        var multipleInstance = instance.ShouldBeOfType<MultipleProblemInstance>();
        multipleInstance.Problems.Length.ShouldBe(2);

        instance.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
        details.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
    }

    [Theory]
    [MemberData(nameof(MergeCounts))]
    public void MergeWith_MergesProblems_AndClearsOther(int leftCount, int rightCount)
    {
        var leftProblems = CreateArray(leftCount);
        var rightProblems = CreateArray(rightCount);
        var left = Create(leftProblems);
        var right = Create(rightProblems);

        left.MergeWith(ref right);

        left.Count.ShouldBe(leftCount + rightCount);
        left.Detail.ShouldBeNull();
        right.Count.ShouldBe(0);
        right.Detail.ShouldBeNull();

        foreach (var problem in leftProblems.Concat(rightProblems))
        {
            left.ShouldContain(problem);
        }
    }

    [Fact]
    public void MergeWith_MergesExtensions_AndPreservesExistingDetail()
    {
        var left = MultipleProblemInstance.CreateBuilder();
        var right = MultipleProblemInstance.CreateBuilder();

        left.Add(CreateDistinctItem());
        left.Detail = "left-detail";
        left.AddExtension("shared", "left");
        left.AddExtension("left-only", "left-value");

        right.Add(CreateDistinctItem());
        right.Detail = "right-detail";
        right.AddExtension("shared", "right");
        right.AddExtension("right-only", "right-value");

        left.MergeWith(ref right);

        left.TryBuild(out var instance).ShouldBeTrue();
        var multiple = instance.ShouldBeOfType<MultipleProblemInstance>();
        multiple.Problems.Length.ShouldBe(2);
        multiple.Detail.ShouldBe("left-detail");
        multiple.Extensions["shared"].ShouldContain("right");
        multiple.Extensions["left-only"].ShouldContain("left-value");
        multiple.Extensions["right-only"].ShouldContain("right-value");

        right.Count.ShouldBe(0);
        right.Detail.ShouldBeNull();
    }

    [Fact]
    public void MergeWith_UsesOtherDetail_WhenThisDetailIsEmpty()
    {
        var left = MultipleProblemInstance.CreateBuilder();
        var right = MultipleProblemInstance.CreateBuilder();

        left.Add(CreateDistinctItem());
        right.Add(CreateDistinctItem());
        right.Detail = "right-detail";

        left.MergeWith(ref right);

        left.TryBuild(out var instance).ShouldBeTrue();
        instance.ShouldBeOfType<MultipleProblemInstance>().Detail.ShouldBe("right-detail");
        right.Detail.ShouldBeNull();
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
