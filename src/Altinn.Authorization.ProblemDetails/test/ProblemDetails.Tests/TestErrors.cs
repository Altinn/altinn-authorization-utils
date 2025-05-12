using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

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
