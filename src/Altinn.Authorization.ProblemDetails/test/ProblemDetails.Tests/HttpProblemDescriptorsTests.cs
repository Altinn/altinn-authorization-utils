using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class HttpProblemDescriptorsTests
{
    [Theory]
    [InlineData(HttpStatusCode.Continue, "HTTP-00100", "Continue")]
    [InlineData(HttpStatusCode.SwitchingProtocols, "HTTP-00101", "Switching Protocols")]
    [InlineData(HttpStatusCode.Processing, "HTTP-00102", "Processing")]
    [InlineData(HttpStatusCode.EarlyHints, "HTTP-00103", "HTTP 103")]
    [InlineData(HttpStatusCode.OK, "HTTP-00200", "OK")]
    [InlineData(HttpStatusCode.Created, "HTTP-00201", "Created")]
    [InlineData(HttpStatusCode.Accepted, "HTTP-00202", "Accepted")]
    [InlineData(HttpStatusCode.NonAuthoritativeInformation, "HTTP-00203", "Non-Authoritative Information")]
    [InlineData(HttpStatusCode.NoContent, "HTTP-00204", "No Content")]
    [InlineData(HttpStatusCode.ResetContent, "HTTP-00205", "Reset Content")]
    [InlineData(HttpStatusCode.PartialContent, "HTTP-00206", "Partial Content")]
    [InlineData(HttpStatusCode.MultiStatus, "HTTP-00207", "Multi-Status")]
    [InlineData(HttpStatusCode.AlreadyReported, "HTTP-00208", "Already Reported")]
    [InlineData(HttpStatusCode.IMUsed, "HTTP-00226", "IM Used")]
    [InlineData(HttpStatusCode.MultipleChoices, "HTTP-00300", "Multiple Choices")]
    [InlineData(HttpStatusCode.MovedPermanently, "HTTP-00301", "Moved Permanently")]
    [InlineData(HttpStatusCode.Found, "HTTP-00302", "Found")]
    [InlineData(HttpStatusCode.SeeOther, "HTTP-00303", "See Other")]
    [InlineData(HttpStatusCode.NotModified, "HTTP-00304", "Not Modified")]
    [InlineData(HttpStatusCode.UseProxy, "HTTP-00305", "Use Proxy")]
    [InlineData(HttpStatusCode.Unused, "HTTP-00306", "Switch Proxy")]
    [InlineData(HttpStatusCode.TemporaryRedirect, "HTTP-00307", "Temporary Redirect")]
    [InlineData(HttpStatusCode.PermanentRedirect, "HTTP-00308", "Permanent Redirect")]
    [InlineData(HttpStatusCode.BadRequest, "HTTP-00400", "Bad Request")]
    [InlineData(HttpStatusCode.Unauthorized, "HTTP-00401", "Unauthorized")]
    [InlineData(HttpStatusCode.PaymentRequired, "HTTP-00402", "Payment Required")]
    [InlineData(HttpStatusCode.Forbidden, "HTTP-00403", "Forbidden")]
    [InlineData(HttpStatusCode.NotFound, "HTTP-00404", "Not Found")]
    [InlineData(HttpStatusCode.MethodNotAllowed, "HTTP-00405", "Method Not Allowed")]
    [InlineData(HttpStatusCode.NotAcceptable, "HTTP-00406", "Not Acceptable")]
    [InlineData(HttpStatusCode.ProxyAuthenticationRequired, "HTTP-00407", "Proxy Authentication Required")]
    [InlineData(HttpStatusCode.RequestTimeout, "HTTP-00408", "Request Timeout")]
    [InlineData(HttpStatusCode.Conflict, "HTTP-00409", "Conflict")]
    [InlineData(HttpStatusCode.Gone, "HTTP-00410", "Gone")]
    [InlineData(HttpStatusCode.LengthRequired, "HTTP-00411", "Length Required")]
    [InlineData(HttpStatusCode.PreconditionFailed, "HTTP-00412", "Precondition Failed")]
    [InlineData(HttpStatusCode.RequestEntityTooLarge, "HTTP-00413", "Content Too Large")]
    [InlineData(HttpStatusCode.RequestUriTooLong, "HTTP-00414", "URI Too Long")]
    [InlineData(HttpStatusCode.UnsupportedMediaType, "HTTP-00415", "Unsupported Media Type")]
    [InlineData(HttpStatusCode.RequestedRangeNotSatisfiable, "HTTP-00416", "Range Not Satisfiable")]
    [InlineData(HttpStatusCode.ExpectationFailed, "HTTP-00417", "Expectation Failed")]
    [InlineData(HttpStatusCode.MisdirectedRequest, "HTTP-00421", "Misdirected Request")]
    [InlineData(HttpStatusCode.UnprocessableEntity, "HTTP-00422", "Unprocessable Entity")]
    [InlineData(HttpStatusCode.Locked, "HTTP-00423", "Locked")]
    [InlineData(HttpStatusCode.FailedDependency, "HTTP-00424", "Failed Dependency")]
    [InlineData(HttpStatusCode.UpgradeRequired, "HTTP-00426", "Upgrade Required")]
    [InlineData(HttpStatusCode.PreconditionRequired, "HTTP-00428", "Precondition Required")]
    [InlineData(HttpStatusCode.TooManyRequests, "HTTP-00429", "Too Many Requests")]
    [InlineData(HttpStatusCode.RequestHeaderFieldsTooLarge, "HTTP-00431", "Request Header Fields Too Large")]
    [InlineData(HttpStatusCode.UnavailableForLegalReasons, "HTTP-00451", "Unavailable For Legal Reasons")]
    [InlineData(HttpStatusCode.InternalServerError, "HTTP-00500", "An error occurred while processing your request.")]
    [InlineData(HttpStatusCode.NotImplemented, "HTTP-00501", "Not Implemented")]
    [InlineData(HttpStatusCode.BadGateway, "HTTP-00502", "Bad Gateway")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "HTTP-00503", "Service Unavailable")]
    [InlineData(HttpStatusCode.GatewayTimeout, "HTTP-00504", "Gateway Timeout")]
    [InlineData(HttpStatusCode.HttpVersionNotSupported, "HTTP-00505", "HTTP Version Not Supported")]
    [InlineData(HttpStatusCode.VariantAlsoNegotiates, "HTTP-00506", "Variant Also Negotiates")]
    [InlineData(HttpStatusCode.InsufficientStorage, "HTTP-00507", "Insufficient Storage")]
    [InlineData(HttpStatusCode.LoopDetected, "HTTP-00508", "Loop Detected")]
    [InlineData(HttpStatusCode.NotExtended, "HTTP-00510", "Not Extended")]
    [InlineData(HttpStatusCode.NetworkAuthenticationRequired, "HTTP-00511", "Network Authentication Required")]
    [InlineData((HttpStatusCode)420, "HTTP-00420", "HTTP 420")]
    [InlineData((HttpStatusCode)600, "HTTP-00600", "HTTP 600")]
    [InlineData((HttpStatusCode)999, "HTTP-00999", "HTTP 999")]
    public void For_CreatesExpectedDescriptor(
        HttpStatusCode statusCode,
        string expectedErrorCode,
        string expectedDescription)
    {
        var descriptor = HttpProblemDescriptors.For(statusCode);

        descriptor.StatusCode.ShouldBe(statusCode);
        descriptor.ErrorCode.ToString().ShouldBe(expectedErrorCode);
        descriptor.Title.ShouldBe(expectedDescription);
    }
}
