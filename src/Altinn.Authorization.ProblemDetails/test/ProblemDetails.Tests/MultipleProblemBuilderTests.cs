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
}
