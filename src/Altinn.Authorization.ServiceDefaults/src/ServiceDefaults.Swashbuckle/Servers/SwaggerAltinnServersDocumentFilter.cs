using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Servers;

[ExcludeFromCodeCoverage]
internal sealed class SwaggerAltinnServersDocumentFilter
    : IDocumentFilter
{
    private readonly IOptionsMonitor<AltinnServerOptions> _options;
    private readonly ImmutableArray<string> _localAddresses;

    public SwaggerAltinnServersDocumentFilter(IOptionsMonitor<AltinnServerOptions> options, IServer server)
    {
        _options = options;
        _localAddresses = server.Features.Get<IServerAddressesFeature>()?.Addresses?.ToImmutableArray() ?? [];
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var options = _options.Get(context.DocumentName);
        var defaultOptions = _options.CurrentValue;
        var suffix = options.EnvironmentServerPathSuffix.HasValue ? options.EnvironmentServerPathSuffix : defaultOptions.EnvironmentServerPathSuffix;

        if (options.IncludeLocalhostServer ?? defaultOptions.IncludeLocalhostServer ?? true) 
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Description = "Local",
                Url = "{address}/",
                Variables = new Dictionary<string, OpenApiServerVariable>
                {
                    {
                        "address",
                        new OpenApiServerVariable
                        {
                            Default = _localAddresses[0],
                            Enum = [.. _localAddresses],
                        }
                    }
                }
            });
        }

        if (options.IncludeAcceptanceTestServers ?? defaultOptions.IncludeAcceptanceTestServers ?? true)
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Description = "Acceptance Test",
                Url = string.Concat("https://platform.{environment}.altinn.cloud/", suffix),
                Variables = new Dictionary<string, OpenApiServerVariable> {
                    { 
                        "environment", 
                        new OpenApiServerVariable 
                        {
                            Default = "at22",
                            Enum = ["at22", "at23", "at24"],
                        }
                    },
                },
            });
        }

        if (options.IncludePerformanceTestServer ?? defaultOptions.IncludePerformanceTestServer ?? true)
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Description = "Performance Test",
                Url = string.Concat("https://platform.yt01.altinn.cloud/", suffix),
            });
        }

        if (options.IncludeIntegrationTestServer ?? defaultOptions.IncludeIntegrationTestServer ?? true)
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Description = "Integration Test",
                Url = string.Concat("https://platform.tt02.altinn.no/", suffix),
            });
        }

        if (options.IncludeProductionServers ?? defaultOptions.IncludeProductionServers ?? true)
        {
            swaggerDoc.Servers.Add(new OpenApiServer
            {
                Description = "Production",
                Url = string.Concat("https://platform.altinn.no/", suffix),
            });
        }
    }
}
