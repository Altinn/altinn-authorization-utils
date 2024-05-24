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
            deserialized.Detail.Should().Be(problemDetails.Detail);
            deserialized.Status.Should().Be(problemDetails.Status);
            deserialized.ErrorCode.Should().Be(problemDetails.ErrorCode);
        }
    }

    [Fact]
    public void SetsStatusCode()
    {
        TestErrors.BadRequest.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        TestErrors.NotFound.StatusCode.Should().Be(HttpStatusCode.NotFound);
        TestErrors.InternalServerError.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        TestErrors.NotImplemented.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }

    [Fact]
    public void SetsDetail()
    {
        TestErrors.BadRequest.Detail.Should().Be("Bad request");
        TestErrors.NotFound.Detail.Should().Be("Not found");
        TestErrors.InternalServerError.Detail.Should().Be("Internal server error");
        TestErrors.NotImplemented.Detail.Should().Be("Not implemented");
    }

    internal static class TestErrors
    {
        private static readonly ProblemDescriptorFactory _factory
            = ProblemDescriptorFactory.New("TEST");

        public static ProblemDescriptor BadRequest { get; }
            = _factory.Create(1, HttpStatusCode.BadRequest, "Bad request");

        public static ProblemDescriptor NotFound { get; }
            = _factory.Create(2, HttpStatusCode.NotFound, "Not found");

        public static ProblemDescriptor InternalServerError { get; }
            = _factory.Create(3, HttpStatusCode.InternalServerError, "Internal server error");

        public static ProblemDescriptor NotImplemented { get; }
            = _factory.Create(4, HttpStatusCode.NotImplemented, "Not implemented");
    }
}
