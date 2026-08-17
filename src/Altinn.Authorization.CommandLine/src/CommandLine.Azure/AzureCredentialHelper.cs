using System.Web;
using Azure.Core;
using Azure.Identity;

namespace Altinn.Authorization.CommandLine.Azure;

/// <summary>
/// Provides helper methods for obtaining Azure credentials based on a URI.
/// </summary>
public static class AzureCredentialHelper
{
    /// <summary>
    /// Gets an Azure credential based on the specified URI. The URI can contain query parameters for subscriptionId and tenantId.
    /// </summary>
    /// <param name="uri">The URI containing query parameters for subscriptionId and tenantId.</param>
    /// <returns>An Azure TokenCredential.</returns>
    public static TokenCredential GetCredential(Uri? uri)
    {
        var options = new AzureCliCredentialOptions();

        if (uri is not null)
        {
            var query = uri.Query.Length > 1 ? HttpUtility.ParseQueryString(uri.Query[1..]) : [];

            if (query.Get("subscriptionId") is { Length: > 0 } subscriptionId)
            {
                options.Subscription = subscriptionId;
            }
            else if (query.Get("tenantId") is { Length: > 0 } tenantId)
            {
                options.TenantId = tenantId;
            }
        }

        return new BugFixCredential(options);
    }

    // https://github.com/Azure/azure-sdk-for-net/issues/58949
    private sealed class BugFixCredential
        : AzureCliCredential
    {
        private bool _hasSubscription = false;

        public BugFixCredential(AzureCliCredentialOptions options)
            : base(options)
        {
            _hasSubscription = options.Subscription is not null;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            FixBug(ref requestContext);
            return base.GetToken(requestContext, cancellationToken);
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            FixBug(ref requestContext);
            return base.GetTokenAsync(requestContext, cancellationToken);
        }

        private void FixBug(ref TokenRequestContext requestContext)
        {
            if (!_hasSubscription)
            {
                // not bugged
                return;
            }

            if (string.IsNullOrEmpty(requestContext.TenantId))
            {
                // no conflict
                return;
            }

            requestContext = new TokenRequestContext(
                scopes: requestContext.Scopes,
                parentRequestId: requestContext.ParentRequestId,
                claims: requestContext.Claims,
                tenantId: null, // <- this is the fix
                isCaeEnabled: requestContext.IsCaeEnabled,
                isProofOfPossessionEnabled: requestContext.IsProofOfPossessionEnabled,
                proofOfPossessionNonce: requestContext.ProofOfPossessionNonce,
                requestUri: requestContext.ResourceRequestUri,
                requestMethod: requestContext.ResourceRequestMethod);
        }
    }
}
