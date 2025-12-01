using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// Provides extension methods for configuring Altinn-specific authorization policies using the
/// AuthorizationPolicyBuilder.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AltinnAuthorizationPolicyBuilderExtensions
{
    /// <param name="builder">The <see cref="AuthorizationPolicyBuilder"/> to which the scope requirement will be added.</param>
    extension(AuthorizationPolicyBuilder builder)
    {

#if NET9_0_OR_GREATER
        /// <summary>
        /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
        /// user's claims.
        /// </summary>
        /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
        /// requirement. Cannot be null or empty.</param>
        /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
        [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
        public AuthorizationPolicyBuilder RequireScopeAnyOf(params ReadOnlySpan<string> scopes)
        {
            if (scopes.IsEmpty)
            {
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
            }

            builder.Requirements.Add(new ScopeAnyOfAuthorizationRequirement(scopes));
            return builder;
        }
#endif

        /// <summary>
        /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
        /// user's claims.
        /// </summary>
        /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
        /// requirement. Cannot be null or empty.</param>
        /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
        public AuthorizationPolicyBuilder RequireScopeAnyOf(params string[] scopes)
        {
            if (scopes is null or { Length: 0 })
            {
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
            }

            builder.Requirements.Add(new ScopeAnyOfAuthorizationRequirement(scopes));
            return builder;
        }

        /// <summary>
        /// Adds a requirement to the authorization policy that enforces the presence of a valid platform access token.
        /// </summary>
        /// <returns>The same <see cref="AuthorizationPolicyBuilder"/> instance, enabling method chaining.</returns>
        public AuthorizationPolicyBuilder RequirePlatformAccessToken()
        {
            builder.Requirements.Add(PlatformAccessTokenRequirement.AllowAll);
            return builder;
        }

        /// <summary>
        /// Adds a requirement to the authorization policy that enforces the presence of a valid platform access token.
        /// </summary>
        /// <param name="approvedIssuers">The approved issuers for the platform access token.</param>
        /// <returns>The same <see cref="AuthorizationPolicyBuilder"/> instance, enabling method chaining.</returns>
        public AuthorizationPolicyBuilder RequirePlatformAccessToken(WellKnownPlatformAccessTokenIssuers approvedIssuers)
        {
            builder.Requirements.Add(new PlatformAccessTokenRequirement(ApprovedIssuersCheck.Create(approvedIssuers)));
            return builder;
        }

#if NET9_0_OR_GREATER
        /// <summary>
        /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
        /// user's claims, *or* that a platform-access-token is included in the request.
        /// </summary>
        /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
        /// requirement. Cannot be null or empty.</param>
        /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
        [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
        public AuthorizationPolicyBuilder RequirePlatformAccessTokenOrScopeAnyOf(params ReadOnlySpan<string> scopes)
        {
            if (scopes.IsEmpty)
            {
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
            }

            builder.Requirements.Add(new PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement(scopes));
            return builder;
        }

        /// <summary>
        /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
        /// user's claims, *or* that a platform-access-token is included in the request.
        /// </summary>
        /// <param name="approvedIssuers">The approved issuers for the platform access token.</param>
        /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
        /// requirement. Cannot be null or empty.</param>
        /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
        [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
        public AuthorizationPolicyBuilder RequirePlatformAccessTokenOrScopeAnyOf(WellKnownPlatformAccessTokenIssuers approvedIssuers, params ReadOnlySpan<string> scopes)
        {
            if (scopes.IsEmpty)
            {
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
            }

            builder.Requirements.Add(new PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement(approvedIssuers, scopes));
            return builder;
        }
#endif

        /// <summary>
        /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
        /// user's claims, *or* that a platform-access-token is included in the request.
        /// </summary>
        /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
        /// requirement. Cannot be null or empty.</param>
        /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
        public AuthorizationPolicyBuilder RequirePlatformAccessTokenOrScopeAnyOf(params string[] scopes)
        {
            if (scopes is null or { Length: 0 })
            {
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
            }

            builder.Requirements.Add(new PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement(scopes));
            return builder;
        }

        /// <summary>
        /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
        /// user's claims, *or* that a platform-access-token is included in the request.
        /// </summary>
        /// <param name="approvedIssuers">The approved issuers for the platform access token.</param>
        /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
        /// requirement. Cannot be null or empty.</param>
        /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
        public AuthorizationPolicyBuilder RequirePlatformAccessTokenOrScopeAnyOf(WellKnownPlatformAccessTokenIssuers approvedIssuers, params string[] scopes)
        {
            if (scopes is null or { Length: 0 })
            {
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
            }

            builder.Requirements.Add(new PlatformAccessTokenOrScopeAnyOfAuthorizationRequirement(approvedIssuers, scopes));
            return builder;
        }
    }
}
