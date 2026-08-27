using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model.Utils;

internal static class ThrowHelperExtensions
{
    extension(ThrowHelper)
    {
        [DoesNotReturn]
        public static void ThrowFileNotFoundException(string message)
        {
            throw new FileNotFoundException(message);
        }
    }
}
