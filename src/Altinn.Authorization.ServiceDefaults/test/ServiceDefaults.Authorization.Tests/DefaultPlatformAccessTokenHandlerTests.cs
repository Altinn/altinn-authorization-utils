using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;
using Altinn.Authorization.ServiceDefaults.Authorization.Tests.Mocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests;

public class DefaultPlatformAccessTokenHandlerTests
{
    private const string DefaultIssuer = "test-issuer";
    private const string DefaultApp = "test-app";
    private const string HeaderName = "PlatformAccessToken";

    private readonly TestPlatformAccessTokenSigningKeyProvider _provider = new();
    private readonly PlatformAccessTokenSettings _settings = new();
    private readonly IServiceProvider _services;

    private readonly DefaultPlatformAccessTokenHandler _sut;

    public DefaultPlatformAccessTokenHandlerTests()
    {
        _services = new ServiceCollection()
            .AddSingleton<IPlatformAccessTokenSigningKeyProvider>(_provider)
            .BuildServiceProvider();

        _sut = new(
            settings: new TestOptionsMonitor<PlatformAccessTokenSettings>(_settings),
            logger: new NullLogger<DefaultPlatformAccessTokenHandler>(),
            serviceProvider: _services);
    }

    [Fact]
    public async Task HandleRequirementAsync_Disabled_ContextFail()
    {
        _settings.Enable = false;

        var token = CreateToken();

        var context = CreateContext(token);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoToken_ContextFail()
    {
        var context = CreateContext(token: null);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_Token_IssuerNotInProvider_ContextFail()
    {
        var token = CreateToken();
        _provider.Delete(DefaultIssuer);

        var context = CreateContext(token);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_Token_IssuerNotSupported_ContextFail()
    {
        var token = CreateToken();

        var context = CreateContext(token, approvedIssuers: ["some-other-issuer"]);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_Token_AnyIssuer_ContextSucceeded()
    {
        var token = CreateToken();

        var context = CreateContext(token);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_Token_ExactIssuer_ContextSucceeded()
    {
        var token = CreateToken();

        var context = CreateContext(token, approvedIssuers: [DefaultIssuer]);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_Token_MultipleIssuer_ContextSucceeded()
    {
        var token = CreateToken();

        var context = CreateContext(token, approvedIssuers: ["other", DefaultIssuer, "issuer"]);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_Token_MultipleCertVersions()
    {
        var token = CreateToken();
        _provider.CreateOrRotateCertificate(DefaultIssuer); // Create a new version of the cert

        var context = CreateContext(token);

        await _sut.HandleAsync(context);

        context.HasSucceeded.ShouldBeTrue();
    }

    private string CreateToken(string issuer = DefaultIssuer, string app = DefaultApp)
    {
        var cert = _provider.GetOrCreateCertificate(issuer);
        var creds = new X509SigningCredentials(cert);
        var token = new JwtSecurityToken(
            issuer: issuer,
            claims: [new Claim(DefaultPlatformAccessTokenHandler.AppClaim, app)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthorizationHandlerContext CreateContext(
        string? token,
        scoped ReadOnlySpan<string> approvedIssuers = default)
    {
        var requirement = new PlatformAccessTokenRequirement(ApprovedIssuersCheck.Create(approvedIssuers));

        ClaimsPrincipal user = new([
            new ClaimsIdentity(
                [
                ],
                "AuthenticationTypes.Federation"
            )
        ]);

        HttpContext httpContext = new DefaultHttpContext()
        {
            User = user,
        };

        if (!string.IsNullOrEmpty(token))
        {
            httpContext.Request.Headers.Append(HeaderName, token);
        }
        
        AuthorizationHandlerContext authContext = new(
            [requirement],
            user,
            httpContext);

        return authContext;
    }
}
