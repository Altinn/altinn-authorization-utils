using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.CommandLine.Shell;

internal static class ShellThrowHelperExtensions
{
    extension(ThrowHelper)
    {
        [DoesNotReturn]
        public static void ThrowProcessFailedException(Process process, string filename)
        {
            throw new ProcessFailedException(process, $"Process '{filename}' exited with code {process.ExitCode}.");
        }
    }
}
