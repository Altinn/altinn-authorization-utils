using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Altinn.Authorization.TestUtils.AspNetCore.Authentication;

internal sealed class TestSchemeHandler(IOptionsMonitor<TestSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<TestSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.TryGetValue("X-TestClient-User", out var value) && value.Count == 1)
        {
            var user = Json.Deserialize<JsonClaimsPrincipal>(value[0]!);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user!.ToPrincipal(), Scheme.Name)));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
