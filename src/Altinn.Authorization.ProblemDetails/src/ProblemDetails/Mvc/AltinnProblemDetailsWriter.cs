using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Altinn.Authorization.ProblemDetails.Mvc;

internal sealed class AltinnProblemDetailsWriter
    : IProblemDetailsWriter
{
    private const string ProblemDetailsContentType = "application/problem+json";

    private static readonly MediaTypeHeaderValue _jsonMediaType = new("application/json");
    private static readonly MediaTypeHeaderValue _problemDetailsJsonMediaType = new(ProblemDetailsContentType);

    private readonly JsonSerializerOptions _serializerOptions;

    public AltinnProblemDetailsWriter(IOptions<JsonOptions> jsonOptions)
    {
        _serializerOptions = jsonOptions.Value.SerializerOptions;
    }

    public bool CanWrite(ProblemDetailsContext context)
    {
        if (context.ProblemDetails is not AltinnProblemDetails)
        {
            return false;
        }

        var httpContext = context.HttpContext;
        var acceptHeader = httpContext.Request.Headers.Accept.GetMediaTypeHeaderValueList();

        // Based on https://www.rfc-editor.org/rfc/rfc7231#section-5.3.2 a request
        // without the Accept header implies that the user agent
        // will accept any media type in response
        if (acceptHeader.Count == 0)
        {
            return true;
        }

        for (var i = 0; i < acceptHeader.Count; i++)
        {
            var acceptHeaderValue = acceptHeader[i];
            // Check to see if the Accepted header values support `application/json` or `application/problem+json`
            // with  support for argument parameters. Support handling `*/*` and `application/*` as Accepts header values.
            // Application/json is a subset of */* but */* is not a subset of application/json
            if (acceptHeaderValue.IsSubsetOf(_jsonMediaType) || acceptHeaderValue.IsSubsetOf(_problemDetailsJsonMediaType) || _jsonMediaType.IsSubsetOf(acceptHeaderValue))
            {
                return true;
            }
        }

        return false;
    }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var httpContext = context.HttpContext;

        var problemDetailsType = context.ProblemDetails.GetType();

        return new ValueTask(
            httpContext.Response.WriteAsJsonAsync(
                context.ProblemDetails,
                _serializerOptions.GetTypeInfo(problemDetailsType),
                contentType: ProblemDetailsContentType));
    }
}
