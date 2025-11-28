using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

internal sealed partial class DefaultPlatformAccessTokenHandler
    : AuthorizationHandler<IPlatformAccessTokenRequirement>
{
    internal const string AppClaim = "urn:altinn:app";
    private static readonly TimeSpan ClockSkew = new TimeSpan(0, 0, 10);

    private readonly IOptionsMonitor<PlatformAccessTokenSettings> _settings;
    private readonly ILogger<DefaultPlatformAccessTokenHandler> _logger;
    private readonly IPlatformAccessTokenSigningKeyProvider _keyProvider;

    public DefaultPlatformAccessTokenHandler(
        IOptionsMonitor<PlatformAccessTokenSettings> settings,
        ILogger<DefaultPlatformAccessTokenHandler> logger,
        IPlatformAccessTokenSigningKeyProvider keyProvider)
    {
        _settings = settings;
        _logger = logger;
        _keyProvider = keyProvider;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IPlatformAccessTokenRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            return HandleRequirementAsync(httpContext, context, requirement, httpContext.RequestAborted);
        }

        return Task.CompletedTask;
    }

    private Task HandleRequirementAsync(
        HttpContext httpContext,
        AuthorizationHandlerContext context,
        IPlatformAccessTokenRequirement requirement,
        CancellationToken cancellationToken)
    {
        var settings = _settings.CurrentValue;
        if (!httpContext.Request.Headers.TryGetValue(settings.AccessTokenHeaderId, out StringValues tokens)
            || tokens.Count == 0
            || string.IsNullOrEmpty(tokens[0]))
        {
            if (settings.DisableAccessTokenVerification)
            {
                Log.TokenMissingButVerificationDisabled(_logger);
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            Log.TokenMissing(_logger);
            return Task.CompletedTask;
        }

        if (tokens.Count > 1)
        {
            Log.MultipleTokens(_logger);
            return Task.CompletedTask;
        }

        return HandleTokenAsync(tokens[0]!, context, requirement, settings, httpContext, cancellationToken);
    }

    private async Task HandleTokenAsync(
        string token,
        AuthorizationHandlerContext context,
        IPlatformAccessTokenRequirement requirement,
        PlatformAccessTokenSettings settings,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        bool isValid;

        try
        {
            isValid = await ValidateAccessTokenAsync(token, requirement.ApprovedIssuers, settings, httpContext, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.AccessTokenValidationFailed(_logger, ex);
            isValid = false;
        }

        if (isValid)
        {
            context.Succeed(requirement);
        }
        else if (settings.DisableAccessTokenVerification)
        {
            Log.InvalidTokenButDisabled(_logger);
            context.Succeed(requirement);
        }
    }

    private async ValueTask<bool> ValidateAccessTokenAsync(
        string token,
        ApprovedIssuersCheck approvedIssuers,
        PlatformAccessTokenSettings settings,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        JwtSecurityTokenHandler handler = new();

        if (!handler.CanReadToken(token))
        {
            Log.CannotReadToken(_logger);
            return false;
        }

        // Read the JWT token to extract the issuer
        var jwt = handler.ReadJwtToken(token);
        Log.Issuer(_logger, jwt.Issuer);

        // Validate issuer
        if (!approvedIssuers.Check(jwt.Issuer))
        {
            Log.InvalidIssuer(_logger, jwt.Issuer);
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = ClockSkew,
            IssuerSigningKeys = await _keyProvider.GetSigningKeys(jwt.Issuer, cancellationToken).ToListAsync(cancellationToken),
        };

        ClaimsPrincipal principal;
        SecurityToken validated;
        try
        {
            principal = handler.ValidateToken(token, tokenValidationParameters, out validated);
        }
        catch (SecurityTokenValidationException ex)
        {
            Log.InvalidToken(_logger, ex);
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();
        SetAccessTokenCredential(validated.Issuer, principal.Identities.First(), settings, httpContext);

        Log.TokenValid(_logger);
        return true;
    }

    private void SetAccessTokenCredential(
        string issuer,
        ClaimsIdentity identity,
        PlatformAccessTokenSettings settings,
        HttpContext httpContext)
    {
        string? appClaim = identity.FindFirst(type: AppClaim)?.Value;

        httpContext.Items.TryAdd(settings.AccessTokenHttpContextId, $"{issuer}/{appClaim}");
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Access token is missing, but verification is disabled. Authorization requirement succeeded.")]
        public static partial void TokenMissingButVerificationDisabled(ILogger logger);

        [LoggerMessage(1, LogLevel.Information, "Access token is missing. Authorization requirement failed.")]
        public static partial void TokenMissing(ILogger logger);

        [LoggerMessage(2, LogLevel.Warning, "Multiple access tokens found in request header. Authorization requirement failed.")]
        public static partial void MultipleTokens(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Access token validation failed with exception.")]
        public static partial void AccessTokenValidationFailed(ILogger logger, Exception exception);

        [LoggerMessage(4, LogLevel.Information, "Access token is invalid, but verification is disabled. Authorization requirement succeeded.")]
        public static partial void InvalidTokenButDisabled(ILogger logger);

        [LoggerMessage(5, LogLevel.Trace, "Cannot read access token.")]
        public static partial void CannotReadToken(ILogger logger);

        [LoggerMessage(6, LogLevel.Debug, "Access token issuer: {Issuer}")]
        public static partial void Issuer(ILogger logger, string issuer);

        [LoggerMessage(7, LogLevel.Information, "Access token has invalid issuer: {Issuer}")]
        public static partial void InvalidIssuer(ILogger logger, string issuer);

        [LoggerMessage(8, LogLevel.Information, "Access token is invalid. Validation failed with exception.")]
        public static partial void InvalidToken(ILogger logger, Exception? exception);

        [LoggerMessage(9, LogLevel.Debug, "Access token is valid. Authorization requirement succeeded.")]
        public static partial void TokenValid(ILogger logger);
    }
}
