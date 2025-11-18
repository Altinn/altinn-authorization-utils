using Altinn.Swashbuckle.Security;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Swashbuckle.Filters;

[ExcludeFromCodeCoverage]
internal sealed class SwaggerSecurityOperationFilter
    : IOperationAsyncFilter
{
    private readonly OpenApiSecurityProvider _securityProvider;

    public SwaggerSecurityOperationFilter(OpenApiSecurityProvider securityProvider)
    {
        _securityProvider = securityProvider;
    }

    public Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
    {
        var securityContext = new OpenApiSecurityContext { DocumentName = context.DocumentName };
        var task = _securityProvider.GetOperationSecurityInfo(context.ApiDescription, securityContext, cancellationToken);
        if (!task.IsCompletedSuccessfully)
        {
            return WaitAndApply(task, operation, context, cancellationToken);
        }

        Apply(task.Result, operation, context);
        return Task.CompletedTask;
    }

    private async Task WaitAndApply(
        ValueTask<SecurityInfo> securityInfo,
        OpenApiOperation operation,
        OperationFilterContext context,
        CancellationToken cancellationToken)
    {
        Apply(await securityInfo, operation, context);
    }

    private void Apply(
        SecurityInfo securityInfo,
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var security = securityInfo
            .Normalized()
            .Select(req =>
            {
                var requirement = new OpenApiSecurityRequirement();

                foreach (var (scheme, scopes) in req)
                {
                    requirement.Add(
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = scheme } },
                        scopes
                    );
                }

                return requirement;
            });

        operation.Security = [.. security];
    }
}
