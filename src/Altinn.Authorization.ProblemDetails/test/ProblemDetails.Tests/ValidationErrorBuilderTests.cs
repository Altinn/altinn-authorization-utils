namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationErrorBuilderTests
    : CollectionTests<ValidationErrorInstance, ValidationProblemBuilder, CollectionBuilderEnumerator<ValidationErrorInstance>>
{
    private readonly ValidationErrorDescriptorFactory _factory = ValidationErrorDescriptorFactory.New("TEST");
    private uint _nextId = 0;

    protected override ValidationProblemBuilder Create(ReadOnlySpan<ValidationErrorInstance> items)
    {
        var builder = ValidationProblemInstance.CreateBuilder();

        foreach (ref readonly var item in items)
        {
            builder.Add(item);
        }

        return builder;
    }

    protected override ValidationErrorInstance CreateDistinctItem()
    {
        var id = _nextId++;

        return _factory.Create(id, $"Validation error {id}").Create("/path");
    }

    protected override CollectionBuilderEnumerator<ValidationErrorInstance> GetEnumerator(ValidationProblemBuilder list)
    => list.GetEnumerator();

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = ValidationProblemInstance.CreateBuilder();

        errors.TryBuild(out var instance).ShouldBeFalse();
        errors.TryToProblemDetails(out var details).ShouldBeFalse();
        errors.TryToActionResult(out var result).ShouldBeFalse();

        instance.ShouldBeNull();
        details.ShouldBeNull();
        result.ShouldBeNull();
    }

    [Fact]
    public void Errors_TryTo_Returns_True()
    {
        var errors = ValidationProblemInstance.CreateBuilder();

        errors.Add(StdValidationErrors.Required, "/path");
        errors.Add(StdValidationErrors.Required, "/path2");

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();
    }

    [Theory]
    [MemberData(nameof(MergeCounts))]
    public void MergeWith_MergesErrors_AndClearsOther(int leftCount, int rightCount)
    {
        var leftErrors = CreateArray(leftCount);
        var rightErrors = CreateArray(rightCount);
        var left = Create(leftErrors);
        var right = Create(rightErrors);

        left.MergeWith(ref right);

        left.Count.ShouldBe(leftCount + rightCount);
        left.Detail.ShouldBeNull();
        right.Count.ShouldBe(0);
        right.Detail.ShouldBeNull();

        foreach (var error in leftErrors.Concat(rightErrors))
        {
            left.ShouldContain(error);
        }
    }

    [Fact]
    public void MergeWith_MergesExtensions_AndPreservesExistingDetail()
    {
        var left = ValidationProblemInstance.CreateBuilder();
        var right = ValidationProblemInstance.CreateBuilder();

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
        instance.Errors.Length.ShouldBe(2);
        instance.Detail.ShouldBe("left-detail");
        instance.Extensions["shared"].ShouldContain("right");
        instance.Extensions["left-only"].ShouldContain("left-value");
        instance.Extensions["right-only"].ShouldContain("right-value");

        right.Count.ShouldBe(0);
        right.Detail.ShouldBeNull();
    }

    [Fact]
    public void MergeWith_UsesOtherDetail_WhenThisDetailIsEmpty()
    {
        var left = ValidationProblemInstance.CreateBuilder();
        var right = ValidationProblemInstance.CreateBuilder();

        left.Add(CreateDistinctItem());
        right.Add(CreateDistinctItem());
        right.Detail = "right-detail";

        left.MergeWith(ref right);

        left.TryBuild(out var instance).ShouldBeTrue();
        instance.Detail.ShouldBe("right-detail");
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
