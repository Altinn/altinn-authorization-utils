using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class ParseResultExtensions
{
    /// <inheritdoc cref="ServiceProviderServiceExtensions.GetServices{T}(IServiceProvider)"/>
    public static T? GetService<T>(this ParseResult result)
    {
        Guard.IsNotNull(result);

        return result.GetHost().Services.GetService<T>();
    }

    /// <inheritdoc cref="ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider, Type)"/>
    public static T GetRequiredService<T>(this ParseResult result)
        where T : notnull
    {
        Guard.IsNotNull(result);

        return result.GetHost().Services.GetRequiredService<T>();
    }
}
