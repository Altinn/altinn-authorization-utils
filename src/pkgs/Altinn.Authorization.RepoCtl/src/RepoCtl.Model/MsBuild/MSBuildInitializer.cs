using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

internal static class MSBuildInitializer
{
#pragma warning disable CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    [ModuleInitializer]
#pragma warning restore CA2255 // The 'ModuleInitializer' attribute should not be used in libraries
    public static void Initialize()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }
}
