using System.Net;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.ServiceDefaults;

internal sealed class AltinnExceptionHandler
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemInstance = TryGetProblemInstance(exception);
        if (problemInstance is null)
        {
            var statusCode = httpContext.Response.StatusCode;
            if (statusCode < StatusCodes.Status400BadRequest)
            {
                statusCode = StatusCodes.Status500InternalServerError;
            }

            problemInstance = HttpProblemDescriptors.For((HttpStatusCode)statusCode);
        }

        var problemDetails = problemInstance.ToProblemDetails();
        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception,
        });

        return true;
    }

    private static ProblemInstance? TryGetProblemInstance(Exception exception)
    {
        foreach (var exn in Chain(exception))
        {
            if (exn is AggregateException { InnerExceptions.Count: > 0 } aggregateException)
            {
                return TryCollectProblemInstance(aggregateException.InnerExceptions);
            }

            if (exn is IHasProblem { Problem: { } problem })
            {
                return problem;
            }
        }

        return null;

        static IEnumerable<Exception> Chain(Exception? exception)
        {
            while (exception is not null)
            {
                yield return exception;
                exception = exception.InnerException;
            }
        }

        static ProblemInstance? TryCollectProblemInstance(IEnumerable<Exception> exceptions)
        {
            MultipleProblemBuilder builder = default;

            foreach (var exn in exceptions)
            {
                if (TryGetProblemInstance(exn) is { } problem)
                {
                    builder.Add(problem);
                }
            }

            if (builder.TryBuild(out var result))
            {
                return result;
            }

            return null;
        }
    }
}
