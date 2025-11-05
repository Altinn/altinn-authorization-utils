using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Altinn.Authorization.TestUtils.AspNetCore;
using Altinn.Authorization.TestUtils.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Claims;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests;

public class AnyOfScopeAuthorizationHandlerIntegrationTests
{
    [Theory]
    [InlineData("read", "admin", HttpStatusCode.OK)]
    [InlineData("write", "admin", HttpStatusCode.OK)]
    [InlineData("read", "read", HttpStatusCode.Forbidden)]
    [InlineData("write", "read", HttpStatusCode.Forbidden)]
    [InlineData("read", "read access", HttpStatusCode.OK)]
    [InlineData("write", "read access", HttpStatusCode.Forbidden)]
    [InlineData("read", "write", HttpStatusCode.Forbidden)]
    [InlineData("write", "write", HttpStatusCode.Forbidden)]
    [InlineData("read", "write access", HttpStatusCode.OK)]
    [InlineData("write", "write access", HttpStatusCode.OK)]
    public async Task CheckAuthorization(string path, string scopeString, HttpStatusCode expectedStatusCode)
    {
        await using var client = await CreateClient(scopeString);
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);
        await response.ShouldHaveStatusCode(expectedStatusCode);
    }

    async Task<TestClient> CreateClient(string scopeString)
    {
        var client = await TestClient.CreateControllerClient<Controller>(
            configureHost: builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddAuthorization();
                    services.AddAltinnScopesAuthorizationHandlers();
                });
            });

        client.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("scope", scopeString)], "Test")
        );

        return client;
    }

    [ApiController]
    [AnyOfScopeAuthorization("access", "admin")]
    private class Controller
        : ControllerBase
    {
        [HttpGet("read")]
        [AnyOfScopeAuthorization("read", "write", "admin")]
        public ActionResult Read() => Ok();

        [HttpGet("write")]
        [AnyOfScopeAuthorization("write", "admin")]
        public ActionResult Write() => Ok();
    }
}
