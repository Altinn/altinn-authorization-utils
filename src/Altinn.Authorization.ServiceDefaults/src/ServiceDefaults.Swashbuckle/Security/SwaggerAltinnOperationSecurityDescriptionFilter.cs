using Altinn.Swashbuckle.Security;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;

[ExcludeFromCodeCoverage]
internal sealed class SwaggerAltinnOperationSecurityDescriptionFilter
    : IOperationAsyncFilter
{
    private readonly IOptionsMonitor<AltinnSecurityOptions> _options;
    private readonly OpenApiSecurityProvider _securityProvider;

    public SwaggerAltinnOperationSecurityDescriptionFilter(
        IOptionsMonitor<AltinnSecurityOptions> options,
        OpenApiSecurityProvider securityProvider)
    {
        _options = options;
        _securityProvider = securityProvider;
    }

    public Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
    {
        var options = _options.Get(context.DocumentName);
        var defaultOptions = _options.CurrentValue;

        var enabled = options.EnableAltinnOidcScheme ?? defaultOptions.EnableAltinnOidcScheme ?? AltinnSecurityOptions.DefaultEnableAltinnOidcScheme;
        var schemeName = options.AltinnOidcSchemeName ?? defaultOptions.AltinnOidcSchemeName ?? AltinnSecurityOptions.DefaultAltinnOidcSchemeName;

        if (!enabled || string.IsNullOrEmpty(schemeName))
        {
            return Task.CompletedTask;
        }

        var securityContext = new OpenApiSecurityContext { DocumentName = context.DocumentName };
        var task = _securityProvider.GetOperationSecurityInfo(context.ApiDescription, securityContext, cancellationToken);
        if (!task.IsCompletedSuccessfully)
        {
            return WaitAndApply(task, operation, context, schemeName, cancellationToken);
        }

        Apply(task.Result, operation, context, schemeName);
        return Task.CompletedTask;
    }

    private async Task WaitAndApply(
        ValueTask<SecurityInfo> securityInfo,
        OpenApiOperation operation,
        OperationFilterContext context,
        string schemeName,
        CancellationToken cancellationToken)
    {
        Apply(await securityInfo, operation, context, schemeName);
    }

    private void Apply(
        SecurityInfo securityInfo,
        OpenApiOperation operation,
        OperationFilterContext context,
        string schemeName)
    {
        var newline = '\n';
        var builder = new StringBuilder().Append(newline)
            .Append(newline)
            .Append("### Authorization").Append(newline)
            .Append(newline)
            .Append("Operation requires one of the following sets of scopes:").Append(newline)
            .Append(newline);

        var first = true;
        var scopeGroups = securityInfo.Normalized()
            .Select(group => group.Where(g => g.Key == schemeName).Select(g => g.Value).FirstOrDefault())
            .Where(group => !group.IsDefaultOrEmpty);

        foreach (var group in scopeGroups)
        { 
            first = false;
            builder.Append("- `").AppendJoin("`, `", group).Append('`').Append(newline);
        }

        if (!first)
        {
            var postfix = builder.ToString();
            operation.Description += postfix;
        }
    }
}
