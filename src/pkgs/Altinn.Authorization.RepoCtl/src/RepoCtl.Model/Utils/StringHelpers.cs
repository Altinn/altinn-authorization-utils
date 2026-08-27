using System.Runtime.CompilerServices;

namespace Altinn.Authorization.RepoCtl.Model.Utils;

internal static class StringHelpers
{
    extension(in ReadOnlySpan<char> span)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<char> destination, out int charsWritten)
        {
            if (!span.TryCopyTo(destination))
            {
                charsWritten = 0;
                return false;
            }

            charsWritten = span.Length;
            return true;
        }
    }

    extension(string str)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCopyTo(Span<char> destination, out int charsWritten)
            => str.AsSpan().TryCopyTo(destination, out charsWritten);
    }
}
