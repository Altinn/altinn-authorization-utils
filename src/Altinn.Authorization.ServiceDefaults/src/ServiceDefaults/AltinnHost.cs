using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// Altinn host factories.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AltinnHost
{
    /// <summary>
    /// Creates a new <see cref="WebApplicationBuilder"/> instance pre-configured
    /// with altinn defaults.
    /// </summary>
    /// <param name="name">The service name.</param>
    /// <param name="args">The program arguments.</param>
    /// <returns>A <see cref="WebApplicationBuilder"/>.</returns>
    public static WebApplicationBuilder CreateWebApplicationBuilder(string name, string[] args)
    {
        return WebApplicationBuilder(name, args, null);
    }

    /// <summary>
    /// Creates a new <see cref="WebApplicationBuilder"/> instance pre-configured
    /// with altinn defaults.
    /// </summary>
    /// <param name="name">The service name.</param>
    /// <param name="args">The program arguments.</param>
    /// <param name="configureOptions">
    /// An optional action to configure the default services for the Altinn.
    /// </param>
    /// <returns>A <see cref="WebApplicationBuilder"/>.</returns>
    public static WebApplicationBuilder WebApplicationBuilder(string name, string[] args, Action<AltinnServiceDefaultOptions>? configureOptions)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddAltinnServiceDefaults(name, configureOptions);
        return builder;
    }
}
