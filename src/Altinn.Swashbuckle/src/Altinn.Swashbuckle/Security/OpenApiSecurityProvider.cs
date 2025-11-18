using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Provides security information for OpenAPI operations by aggregating security requirements from multiple providers.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class OpenApiSecurityProvider
{
    private readonly ImmutableArray<IOpenApiOperationSecurityProvider> _operationSecurityProviders;
    private readonly ConcurrentDictionary<(string DocumentName, string ActionId), SecurityInfo> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenApiSecurityProvider"/> class.
    /// </summary>
    public OpenApiSecurityProvider(IEnumerable<IOpenApiOperationSecurityProvider> operationSecurityProviders)
    {
        _operationSecurityProviders = [.. operationSecurityProviders];
    }

    /// <summary>
    /// Retrieves security information for the specified API operation.
    /// </summary>
    /// <param name="apiDescription">The API operation description for which to obtain security information.</param>
    /// <param name="context">The context containing OpenAPI document and security settings relevant to the operation.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns><see cref="SecurityInfo"/> for the API operation.</returns>
    public ValueTask<SecurityInfo> GetOperationSecurityInfo(
        ApiDescription apiDescription,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue((context.DocumentName, apiDescription.ActionDescriptor.Id), out var result))
        {
            return new(result);
        }

        return CreateAndPopulate(apiDescription, context, cancellationToken);

        async ValueTask<SecurityInfo> CreateAndPopulate(
            ApiDescription apiDescription,
            OpenApiSecurityContext context,
            CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                // might have been populated while waiting for the lock
                if (_cache.TryGetValue((context.DocumentName, apiDescription.ActionDescriptor.Id), out var result))
                {
                    return result;
                }

                return _cache[(context.DocumentName, apiDescription.ActionDescriptor.Id)] = await AnalyzeOperationSecurity(apiDescription, context, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }
    }

    private async Task<SecurityInfo> AnalyzeOperationSecurity(
        ApiDescription apiDescription,
        OpenApiSecurityContext context,
        CancellationToken cancellationToken)
    {
        var requirements = await _operationSecurityProviders
            .ToAsyncEnumerable()
            .SelectMany(provider => provider.GetSecurityRequirementsForOperation(apiDescription, context, cancellationToken))
            .ToListAsync(cancellationToken);

        return SecurityInfo.Create(requirements);
    }
}
