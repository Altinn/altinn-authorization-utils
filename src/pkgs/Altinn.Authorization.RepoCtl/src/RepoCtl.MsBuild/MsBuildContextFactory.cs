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
    private static bool _installed = false;
    private static readonly Lock _installLock = new();

    /// <inheritdoc/>
    public IMsBuildContext CreateDesignTimeContext(IDictionary<string, string> globalProperties)
    {
        EnsureInstalled();

        var properties = new Dictionary<string, string>(globalProperties);
        properties["DesignTimeBuild"] = "true";

        return new MsBuildContext(properties, loggerFactory.CreateLogger<MsBuildContext>(), isDesignTime: true);
    }

    /// <inheritdoc/>
    public IMsBuildContext CreateBuildContext(IDictionary<string, string> globalProperties)
    {
        EnsureInstalled();

        var properties = new Dictionary<string, string>(globalProperties);
        return new MsBuildContext(properties, loggerFactory.CreateLogger<MsBuildContext>(), isDesignTime: false);
    }

    private static void EnsureInstalled()
    {
        lock (_installLock)
        {
            if (!_installed)
            {
                Install();
                _installed = true;
            }
        }

        static void Install()
        {
            if (!MSBuildLocator.IsRegistered)
            {
                ReadOnlySpan<string> names = ["MSBUILD_EXE_PATH", "MSBuildExtensionsPath", "MSBuildSDKsPath"];

                // 1. Snapshot environment variables
                var snapshot = new string?[names.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    snapshot[i] = Environment.GetEnvironmentVariable(names[i]);
                }

                // 2. Register MSBuild defaults
                MSBuildLocator.RegisterDefaults();

                // 3. Restore environment variables
                for (int i = 0; i < names.Length; i++)
                {
                    Environment.SetEnvironmentVariable(names[i], snapshot[i]);
                }
            }
        }
    }
}
