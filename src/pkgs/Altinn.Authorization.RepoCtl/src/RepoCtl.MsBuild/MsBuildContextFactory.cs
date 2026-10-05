using Microsoft.Build.Locator;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Creates configured MSBuild contexts.
/// </summary>
public interface IMsBuildContextFactory
{
    /// <summary>
    /// Creates an MSBuild design-time context with the specified global properties.
    /// </summary>
    /// <remarks>
    /// The resulting context cannot be used for building.
    /// </remarks>
    /// <param name="globalProperties">The global properties applied to every project loaded by the context.</param>
    /// <returns>A configured MSBuild context.</returns>
    IMsBuildContext CreateDesignTimeContext(IDictionary<string, string> globalProperties);

    /// <summary>
    /// Creates an MSBuild context with the specified global properties.
    /// </summary>
    /// <param name="globalProperties">The global properties applied to every project loaded by the context.</param>
    /// <returns>A configured MSBuild context.</returns>
    IMsBuildContext CreateBuildContext(IDictionary<string, string> globalProperties);
}

internal sealed class MsBuildContextFactory(ILoggerFactory loggerFactory)
    : IMsBuildContextFactory
{
    /// <inheritdoc/>
    public IMsBuildContext CreateDesignTimeContext(IDictionary<string, string> globalProperties)
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        var properties = new Dictionary<string, string>(globalProperties);
        properties["DesignTimeBuild"] = "true";

        return new MsBuildContext(properties, loggerFactory.CreateLogger<MsBuildContext>(), isDesignTime: true);
    }

    /// <inheritdoc/>
    public IMsBuildContext CreateBuildContext(IDictionary<string, string> globalProperties)
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }

        var properties = new Dictionary<string, string>(globalProperties);
        return new MsBuildContext(properties, loggerFactory.CreateLogger<MsBuildContext>(), isDesignTime: false);
    }
}
