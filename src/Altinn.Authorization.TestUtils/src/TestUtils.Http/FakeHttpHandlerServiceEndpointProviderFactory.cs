using Microsoft.Extensions.ServiceDiscovery;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Altinn.Authorization.TestUtils.Http;

internal sealed class FakeHttpHandlerServiceEndpointProviderFactory
    : IServiceEndpointProviderFactory
{
    private static Provider HttpProvider = new(FakeHttpEndpoint.Http);
    private static Provider HttpsProvider = new(FakeHttpEndpoint.Https);

    public bool TryCreateProvider(ServiceEndpointQuery query, [NotNullWhen(true)] out IServiceEndpointProvider? provider)
    {
        if (query.IncludedSchemes.Contains("https"))
        {
            provider = HttpsProvider;
            return true;
        }

        if (query.IncludedSchemes.Contains("http"))
        {
            provider = HttpProvider;
            return true;
        }

        provider = null;
        return false;
    }

    private sealed class Provider(EndPoint endPoint)
        : IServiceEndpointProvider
    {
        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;

        public ValueTask PopulateAsync(IServiceEndpointBuilder endpoints, CancellationToken cancellationToken)
        {
            endpoints.Endpoints.Add(ServiceEndpoint.Create(endPoint));
            return ValueTask.CompletedTask;
        }
    }
}
