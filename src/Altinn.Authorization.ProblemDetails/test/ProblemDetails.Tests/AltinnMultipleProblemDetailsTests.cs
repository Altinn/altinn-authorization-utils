using System.Text.Json;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class AltinnMultipleProblemDetailsTests
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void HasCorrectErrorCode()
    {
        var problemDetails = new AltinnMultipleProblemDetails();

        problemDetails.ErrorCode.ShouldBe(StdProblemDescriptors.MultipleProblems.ErrorCode);
    }

    [Fact]
    public void CanRoundTripThroughJson()
    {
        var problemDetails = new AltinnMultipleProblemDetails([
            TestErrors.InternalServerError.ToProblemDetails(),
            TestErrors.NotFound.ToProblemDetails(),
            TestErrors.NotImplemented.ToProblemDetails(),
        ]);

        var serialized = JsonSerializer.Serialize(problemDetails, _options);
        var deserialized = JsonSerializer.Deserialize<AltinnMultipleProblemDetails>(serialized, _options);

        Assert.NotNull(deserialized);
        deserialized.Problems.ShouldBeEquivalentTo(problemDetails.Problems);
        deserialized.Status.ShouldBe(problemDetails.Status);
        deserialized.Detail.ShouldBe(problemDetails.Detail);
    }

    [Fact]
    public void CanDeserializeEmptyObject()
    {
        var json = """{}""";

        var deserialized = JsonSerializer.Deserialize<AltinnMultipleProblemDetails>(json, _options);
        Assert.NotNull(deserialized);

        deserialized.ErrorCode.ShouldNotBe(StdProblemDescriptors.MultipleProblems.ErrorCode);
        deserialized.Problems.ShouldBeEmpty();
    }

    [Fact]
    public void Builder_ReturnsInner_WhenJustOneErrror_AndNoExtesnsionNorDetail()
    {
        MultipleProblemBuilder builder = default;

        builder.Add(TestErrors.NotFound, "detail");

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ErrorCode.ShouldBe(TestErrors.NotFound.ErrorCode);
    }

    [Fact]
    public void Builder_ReturnsWrapper_WhenJustOneErrror_WithExtensions()
    {
        MultipleProblemBuilder builder = default;

        builder.Add(TestErrors.NotFound, "detail");
        builder.AddExtension("foo", "bar");

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
        instance.Extensions.ShouldContainKeyAndValue("foo", "bar");
    }

    [Fact]
    public void Builder_ReturnsWrapper_WhenJustOneErrror_WithDetail()
    {
        MultipleProblemBuilder builder = default;

        builder.Add(TestErrors.NotFound, "detail");
        builder.Detail = "detail";

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
        instance.Detail.ShouldBe("detail");
    }
}
