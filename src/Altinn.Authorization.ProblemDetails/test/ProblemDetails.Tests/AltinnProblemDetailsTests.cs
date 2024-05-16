using System.Net;
using System.Text.Json;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class AltinnProblemDetailsTests
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void TestJsonRoundTrip()
    {
        TestRoundTrip(TestErrors.BadRequest);
        TestRoundTrip(TestErrors.NotFound);
        TestRoundTrip(TestErrors.InternalServerError);
        TestRoundTrip(TestErrors.NotImplemented);

        static void TestRoundTrip(AltinnProblemDetails problemDetails)
        {
            var json = JsonSerializer.Serialize(problemDetails, _options);
            var deserialized = JsonSerializer.Deserialize<AltinnProblemDetails>(json, _options);

            Assert.NotNull(deserialized);
            deserialized.Detail.Should().Be(problemDetails.Detail);
            deserialized.Status.Should().Be(problemDetails.Status);
            deserialized.ErrorCode.Should().Be(problemDetails.ErrorCode);
        }
    }

    [Fact]
    public void SetsStatusCode()
    {
        TestErrors.BadRequest.Status.Should().Be((int)HttpStatusCode.BadRequest);
        TestErrors.NotFound.Status.Should().Be((int)HttpStatusCode.NotFound);
        TestErrors.InternalServerError.Status.Should().Be((int)HttpStatusCode.InternalServerError);
        TestErrors.NotImplemented.Status.Should().Be((int)HttpStatusCode.NotImplemented);
    }

    [Fact]
    public void SetsDetail()
    {
        TestErrors.BadRequest.Detail.Should().Be("Bad request");
        TestErrors.NotFound.Detail.Should().Be("Not found");
        TestErrors.InternalServerError.Detail.Should().Be("Internal server error");
        TestErrors.NotImplemented.Detail.Should().Be("Not implemented");
    }
}

internal static class TestErrors
{
    private static readonly AltinnProblemDetailsFactory _factory
        = AltinnProblemDetailsFactory.New("TEST");

    public static AltinnProblemDetails BadRequest
        => _factory.Create(1, HttpStatusCode.BadRequest, "Bad request");

    public static AltinnProblemDetails NotFound
        => _factory.Create(2, HttpStatusCode.NotFound, "Not found");

    public static AltinnProblemDetails InternalServerError
        => _factory.Create(3, HttpStatusCode.InternalServerError, "Internal server error");

    public static AltinnProblemDetails NotImplemented
        => _factory.Create(4, HttpStatusCode.NotImplemented, "Not implemented");
}
