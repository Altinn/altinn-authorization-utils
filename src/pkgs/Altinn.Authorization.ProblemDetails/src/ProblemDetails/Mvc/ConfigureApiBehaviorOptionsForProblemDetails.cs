using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ProblemDetails.Mvc;

internal sealed class ConfigureApiBehaviorOptionsForProblemDetails
    : IConfigureOptions<ApiBehaviorOptions>
{
    private readonly AltinnValidationProblemDetailsFactory _problemDetailsFactory;

    public ConfigureApiBehaviorOptionsForProblemDetails(AltinnValidationProblemDetailsFactory problemDetailsFactory)
    {
        _problemDetailsFactory = problemDetailsFactory;
    }

    public void Configure(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(context);
            return problemDetails.ToActionResult();
        };
    }
}
