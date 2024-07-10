using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// Altinn host factories.
/// </summary>
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
        var builder = WebApplication.CreateBuilder(args);

        builder.AddAltinnServiceDefaults(name);
        return builder;
    }
}
