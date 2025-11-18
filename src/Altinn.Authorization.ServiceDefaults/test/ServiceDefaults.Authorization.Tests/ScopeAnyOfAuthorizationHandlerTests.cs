using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests;

public class ScopeAnyOfAuthorizationHandlerTests
{
    private ScopeAnyOfAuthorizationHandler Sut { get; } 
        = new(new DefaultAuthorizationScopeProvider());

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScope_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext("altinn:appdeploy", ["altinn:appdeploy"], useAltinnScopePrefix: true);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScopeOf2_OneInvalidPresent_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext("altinn:resourceregistry:write altinn:resourceregistry:read", ["altinn:resourceregistry:admin", "altinn:resourceregistry:write"]);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Theory]
    [InlineData("scope:start")]
    [InlineData("scope:mid")]
    [InlineData("scope:end")]
    public async Task HandleAsync_ValidScope_PartOfScopeString_ContextSuccess(string valid)
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext("scope:start scope:mid scope:end", [valid]);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleAsync_InvalidScope_NotWholeWord_ContextFail()
    {
        // Arrange
        AuthorizationHandlerContext context = CreateContext("scope:start scope:mid scope:end", ["scope", "start", ":", "mid", "end"]);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Test case: Valid scope is missing in context
    /// Expected: Context will fail.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScopeOf2_OneInvalidPresent_ContextFail()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext("altinn:resourceregistry:read", ["altinn:resourceregistry:admin", "altinn:resourceregistry:write"]);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Test case: Valid scope claim is included in context.
    /// Expected: Context will succeed.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidScopeOf2_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext("altinn:resourceregistry:write", ["altinn:resourceregistry:admin", "altinn:resourceregistry:write"]);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task HandleAsync_ValidScope_PartOfLongerScope_ContextSuccess()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext("altin:foo:bar:baz altinn:foo:bar altinn:foo", ["altinn:foo:bar"]);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    /// <summary>
    /// Test case: Invalid scope claim is included in context.
    /// Expected: Context will fail.
    /// </summary>
    [Fact]
    public async Task HandleAsync_InvalidScope_ContextFail()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext("altinn:invalid", ["altinn:appdeploy"], useAltinnScopePrefix: true);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    /// <summary>
    /// Test case: Empty scope claim is included in context.
    /// Expected: Context will fail.
    /// </summary>
    [Fact]
    public async Task HandleAsync_EmptyScope_ContextFail()
    {
        // Arrange 
        AuthorizationHandlerContext context = CreateContext(string.Empty, ["altinn:appdeploy"], useAltinnScopePrefix: true);

        // Act
        await Sut.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    private static AuthorizationHandlerContext CreateContext(
        string scopeClaim,
        scoped ReadOnlySpan<string> anyOfScopes,
        bool useAltinnScopePrefix = false)
    {
        var requirement = new ScopeAnyOfAuthorizationRequirement(anyOfScopes);

        ClaimsPrincipal user = new([
            new ClaimsIdentity(
                [
                    new Claim(useAltinnScopePrefix ? "urn:altinn:scope" : "scope", scopeClaim, "string", "org"),
                    new Claim("urn:altinn:org", "brg", "string", "org")
                ],
                "AuthenticationTypes.Federation"
            )
        ]);

        HttpContext httpContext = new DefaultHttpContext()
        {
            User = user,
        };

        AuthorizationHandlerContext authContext = new(
            [requirement],
            user,
            httpContext);

        return authContext;
    }
}
