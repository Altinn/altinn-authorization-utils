using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Frozen;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Provides utility methods for retrieving standard descriptions for HTTP status codes.
/// </summary>
internal static class StatusDescriptions
{
    private static readonly FrozenDictionary<int, string> _descriptions
        // From: https://github.com/dotnet/aspnetcore/blob/44a9f8a83a1d20a3e367d5ec0d82111e84c7e5ec/src/Shared/ProblemDetails/ProblemDetailsDefaults.cs
        = new Dictionary<int, string>
        {
            [400] = "Bad Request",
            [401] = "Unauthorized",
            [403] = "Forbidden",
            [404] = "Not Found",
            [405] = "Method Not Allowed",
            [406] = "Not Acceptable",
            [407] = "Proxy Authentication Required",
            [408] = "Request Timeout",
            [409] = "Conflict",
            [410] = "Gone",
            [411] = "Length Required",
            [412] = "Precondition Failed",
            [413] = "Content Too Large",
            [414] = "URI Too Long",
            [415] = "Unsupported Media Type",
            [416] = "Range Not Satisfiable",
            [417] = "Expectation Failed",
            [421] = "Misdirected Request",
            [422] = "Unprocessable Entity",
            [426] = "Upgrade Required",
            [500] = "An error occurred while processing your request.",
            [501] = "Not Implemented",
            [502] = "Bad Gateway",
            [503] = "Service Unavailable",
            [504] = "Gateway Timeout",
            [505] = "HTTP Version Not Supported",
        }.ToFrozenDictionary();

    public static string? GetStatusDescription(int? statusCode)
    {
        if (statusCode is null)
        {
            return null;
        }

        if (_descriptions.TryGetValue(statusCode.Value, out var description))
        {
            return description;
        }

        return ReasonPhrases.GetReasonPhrase(statusCode.Value);
    }
}
