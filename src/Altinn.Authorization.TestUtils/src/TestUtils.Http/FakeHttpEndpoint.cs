using System.Net;

namespace Altinn.Authorization.TestUtils.Http;

/// <summary>
/// Represents a placeholder endpoint for use in scenarios where a real network endpoint is not required.
/// </summary>
/// <remarks>This class can be used in testing or mocking scenarios where an implementation of EndPoint is needed
/// but no actual network communication should occur. It provides a singleton instance via the Instance
/// property.</remarks>
public sealed class FakeHttpEndpoint
    : EndPoint
{
    /// <summary>
    /// Gets the host name used for routing by the fake message handler.
    /// </summary>
    public static string Host => "fake.example.com";

    /// <summary>
    /// Gets the root path used for routing by the fake message handler.
    /// </summary>
    public static string BasePath => "/fake/root/";

    /// <summary>
    /// Gets the root path used for routing by the fake message handler for https requests. 
    /// Any requests made to the fake message handler will have to be relative to this path 
    /// in order to match any routes.
    /// </summary>
    public static Uri HttpsUri { get; } = new Uri($"https://{Host}{BasePath}");

    /// <summary>
    /// Gets the root path used for routing by the fake message handler for http requests. 
    /// Any requests made to the fake message handler will have to be relative to this path 
    /// in order to match any routes.
    /// </summary>
    public static Uri HttpUri { get; } = new Uri($"http://{Host}{BasePath}");

    /// <summary>
    /// Gets an endpoint representing the HTTPS protocol for use in test scenarios.
    /// </summary>
    /// <remarks>This endpoint can be used to simulate HTTPS network communication in unit tests or
    /// development environments. It is intended for testing purposes and does not establish a real network
    /// connection.</remarks>
    public static EndPoint Https { get; } = new FakeHttpEndpoint(HttpsUri);

    /// <summary>
    /// Gets an endpoint representing the HTTP protocol for use in test scenarios.
    /// </summary>
    /// <remarks>This endpoint can be used to simulate HTTP network communication in unit tests or
    /// development environments. It is intended for testing purposes and does not establish a real network
    /// connection.</remarks>
    public static EndPoint Http { get; } = new FakeHttpEndpoint(HttpUri);

    private readonly Uri _uri;

    private FakeHttpEndpoint(Uri uri)
    {
        _uri = uri;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) 
        => obj is FakeHttpEndpoint other
        && _uri.Equals(other._uri);

    /// <inheritdoc/>
    public override int GetHashCode()
        => _uri.GetHashCode();

    /// <inheritdoc/>
    public override string? ToString()
        => _uri.ToString();
}
