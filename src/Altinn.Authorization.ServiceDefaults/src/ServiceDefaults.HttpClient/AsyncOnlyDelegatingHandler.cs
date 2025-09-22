using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.HttpClient;

/// <summary>
/// Provides a base class for HTTP message handlers that support only asynchronous operations.
/// </summary>
/// <remarks>This class disables synchronous HTTP request processing by overriding the synchronous Send method to
/// throw a NotSupportedException. Derived handlers must implement asynchronous logic using SendAsync. Use this class
/// when creating handlers that should not support synchronous execution.</remarks>
public abstract class AsyncOnlyDelegatingHandler
    : DelegatingHandler
{
    /// <inheritdoc/>
    protected sealed override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return ThrowHelper.ThrowNotSupportedException<HttpResponseMessage>("Synchronous Send is not supported. Use SendAsync instead.");
    }
}
