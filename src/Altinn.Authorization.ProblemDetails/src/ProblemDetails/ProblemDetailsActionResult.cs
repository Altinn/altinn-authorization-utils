using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>.
/// </summary>
internal sealed partial class ProblemDetailsActionResult
    : ActionResult
{
    private readonly Microsoft.AspNetCore.Mvc.ProblemDetails _problemDetails;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDetailsActionResult"/> class.
    /// </summary>
    /// <param name="problemDetails">The <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/> to return to the client.</param>
    public ProblemDetailsActionResult(Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        Guard.IsNotNull(problemDetails);

        _problemDetails = problemDetails;
    }

    /// <inheritdoc/>
    public override Task ExecuteResultAsync(ActionContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ProblemDetailsActionResult>>();
        var errorCode = (_problemDetails as AltinnProblemDetails)?.ErrorCode;
        Log.ReturningProblemDetails(logger, errorCode, _problemDetails.Status, _problemDetails.Detail);

        var httpResult = TypedResults.Problem(_problemDetails);

        return httpResult.ExecuteAsync(context.HttpContext);
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Debug, "Returning ProblemDetails with error code {ErrorCode}, status code {StatusCode}, and description '{Detail}'.")]
        public static partial void ReturningProblemDetails(ILogger logger, ErrorCode? errorCode, int? statusCode, string? detail);
    }
}
