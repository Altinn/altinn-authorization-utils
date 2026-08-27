using Altinn.Authorization.TestUtils.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Authentication;

/// <summary>
/// Extensions for <see cref="AuthenticationBuilder"/> to add test authentication.
/// </summary>
public static class TestClientAuthenticationExtensions
{
    extension(AuthenticationBuilder builder)
    {
        /// <summary>
        /// Adds a test authentication scheme to the authentication builder.
        /// </summary>
        /// <param name="scheme">The scheme name.</param>
        /// <returns><paramref name="builder"/>.</returns>
        public AuthenticationBuilder AddTestAuthentication(string scheme)
            => builder.AddScheme<TestSchemeOptions, TestSchemeHandler>(scheme, configureOptions: null);
    }
}
