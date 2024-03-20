using Microsoft.Build.Locator;
using System.Runtime.CompilerServices;

namespace Altinn.Urn.SourceGenerator.Tests;

public static class ModuleInitializer
{
    private static int _initialized = 0;

    [ModuleInitializer]
    public static void Init()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 0)
        {
            MSBuildLocator.RegisterDefaults();

            VerifierSettings.UseSplitModeForUniqueDirectory();
            UseProjectRelativeDirectory("Snapshots");

            VerifySourceGenerators.Initialize();
        }
    }
}
