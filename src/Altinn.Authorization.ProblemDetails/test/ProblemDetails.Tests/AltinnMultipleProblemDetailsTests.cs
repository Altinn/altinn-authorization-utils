using Microsoft.AspNetCore.Mvc;
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
}
