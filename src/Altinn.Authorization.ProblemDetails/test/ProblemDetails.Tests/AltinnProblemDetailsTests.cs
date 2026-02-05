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

        static void TestRoundTrip(ProblemDescriptor descriptor)
        {
            var problemDetails = descriptor.ToProblemDetails();
            var json = JsonSerializer.Serialize(problemDetails, _options);
            var deserialized = JsonSerializer.Deserialize<AltinnProblemDetails>(json, _options);

            Assert.NotNull(deserialized);
            deserialized.Detail.ShouldBe(problemDetails.Detail);
            deserialized.Status.ShouldBe(problemDetails.Status);
            deserialized.ErrorCode.ShouldBe(problemDetails.ErrorCode);
            deserialized.TraceId.ShouldBe(problemDetails.TraceId);
        }
    }

    [Fact]
    public void SetsStatusCode()
    {
        TestErrors.BadRequest.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        TestErrors.NotFound.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        TestErrors.InternalServerError.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        TestErrors.NotImplemented.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
    }

    [Fact]
    public void SetsDetail()
    {
        TestErrors.BadRequest.Title.ShouldBe("Bad request");
        TestErrors.NotFound.Title.ShouldBe("Not found");
        TestErrors.InternalServerError.Title.ShouldBe("Internal server error");
        TestErrors.NotImplemented.Title.ShouldBe("Not implemented");
    }
}
