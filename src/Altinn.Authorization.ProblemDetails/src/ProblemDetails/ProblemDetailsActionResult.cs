using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An <see cref="ActionResult"/> that returns a <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>.
/// </summary>
internal sealed class ProblemDetailsActionResult
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
        var httpResult = TypedResults.Problem(_problemDetails);

        return httpResult.ExecuteAsync(context.HttpContext);
    }
}
