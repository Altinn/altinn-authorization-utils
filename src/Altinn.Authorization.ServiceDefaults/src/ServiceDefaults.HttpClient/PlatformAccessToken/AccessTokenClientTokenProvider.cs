using Altinn.Common.AccessTokenClient.Services;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// A <see cref="IPlatformAccessTokenProvider"/> that uses an <see cref="IAccessTokenGenerator"/> to generate tokens.
/// </summary>
internal sealed class AccessTokenClientTokenProvider
    : IPlatformAccessTokenProvider
{
    private readonly IAccessTokenGenerator _tokenGenerator;
    private readonly IOptions<AltinnPlatformAccessTokenSettings> _options;

    public AccessTokenClientTokenProvider(
        IAccessTokenGenerator tokenGenerator,
        IOptions<AltinnPlatformAccessTokenSettings> options)
    {
        _tokenGenerator = tokenGenerator;
        _options = options;
    }

    /// <inheritdoc/>
    public ValueTask<string> GetPlatformAccessToken(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        return new(_tokenGenerator.GenerateAccessToken(options.Issuer, options.AppName));
    }
}
