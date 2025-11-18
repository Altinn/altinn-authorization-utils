using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Altinn.Authorization.TestUtils.AspNetCore;
using Altinn.Authorization.TestUtils.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Claims;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests;

public abstract class AnyOfScopeAuthorizationHandlerIntegrationTests<TController>
    where TController : class
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

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    protected async Task<TestClient> CreateClient(string scopeString)
    {
        var client = await TestClient.CreateControllerClient<TController>(
            configureHost: builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddAuthorization();
                    services.AddAltinnScopesAuthorizationHandlers();

                    ConfigureServices(services);
                });
            });

        client.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("scope", scopeString)], "Test")
        );

        return client;
    }


    
}

[Collection(NonParallelCollectionDefinitionClass.CollectionName)]
public class AttributeBasedAnyOfScopeAuthorizationHandlerIntegrationTests
    : AnyOfScopeAuthorizationHandlerIntegrationTests<AttributeBasedAnyOfScopeAuthorizationHandlerIntegrationTests.Controller>
{
    [ApiController]
    [ScopeAnyOfAuthorize("access", "admin")]
    public class Controller
        : ControllerBase
    {
        [HttpGet("read")]
        [ScopeAnyOfAuthorize("read", "write", "admin")]
        public ActionResult Read() => Ok();

        [HttpGet("write")]
        [ScopeAnyOfAuthorize("write", "admin")]
        public ActionResult Write() => Ok();
    }

    [Fact]
    public async Task AttributeScopeSearchValues_IsCached()
    {
        await using var client = await CreateClient("admin");
        var response = await client.GetAsync("read", TestContext.Current.CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var createdCount = ScopeSearchValues.CreatedCount;

        response = await client.GetAsync("read", TestContext.Current.CancellationToken);
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
        var newCreatedCount = ScopeSearchValues.CreatedCount;

        newCreatedCount.ShouldBe(createdCount);
    }
}

public class PolicyBasedAnyOfScopeAuthorizationHandlerIntegrationTests
    : AnyOfScopeAuthorizationHandlerIntegrationTests<PolicyBasedAnyOfScopeAuthorizationHandlerIntegrationTests.Controller>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("access-policy", policy => policy.RequireScopeAnyOf("access", "admin"))
            .AddPolicy("read-policy", policy => policy.RequireScopeAnyOf("read", "write", "admin"))
            .AddPolicy("write-policy", policy => policy.RequireScopeAnyOf("write", "admin"));
    }

    [ApiController]
    [Authorize("access-policy")]
    public class Controller
        : ControllerBase
    {
        [HttpGet("read")]
        [Authorize("read-policy")]
        public ActionResult Read() => Ok();

        [HttpGet("write")]
        [Authorize("write-policy")]
        public ActionResult Write() => Ok();
    }
}

[CollectionDefinition(CollectionName, DisableParallelization = true)]
public class NonParallelCollectionDefinitionClass
{
    public const string CollectionName = "Non-Parallel Collection";
}
