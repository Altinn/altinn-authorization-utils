using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;

internal static class Hex
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Format(uint value, Span<char> destination) 
    {
        Debug.Assert(destination.Length >= 8);

        var lookup = "0123456789abcdef"u8;

        // This is manually loop-unrolled for performance reasons, to avoid the overhead of a loop and bounds checks on each iteration.
        destination[0] = (char)lookup[unchecked((int)((value >> 28) & 0xF))];
        destination[1] = (char)lookup[unchecked((int)((value >> 24) & 0xF))];
        destination[2] = (char)lookup[unchecked((int)((value >> 20) & 0xF))];
        destination[3] = (char)lookup[unchecked((int)((value >> 16) & 0xF))];
        destination[4] = (char)lookup[unchecked((int)((value >> 12) & 0xF))];
        destination[5] = (char)lookup[unchecked((int)((value >> 08) & 0xF))];
        destination[6] = (char)lookup[unchecked((int)((value >> 04) & 0xF))];
        destination[7] = (char)lookup[unchecked((int)((value >> 00) & 0xF))];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Format(ulong value, Span<char> destination)
    {
        Debug.Assert(destination.Length >= 16);

        var lookup = "0123456789abcdef"u8;

        // This is manually loop-unrolled for performance reasons, to avoid the overhead of a loop and bounds checks on each iteration.
        destination[00] = (char)lookup[unchecked((int)((value >> 60) & 0xF))];
        destination[01] = (char)lookup[unchecked((int)((value >> 56) & 0xF))];
        destination[02] = (char)lookup[unchecked((int)((value >> 52) & 0xF))];
        destination[03] = (char)lookup[unchecked((int)((value >> 48) & 0xF))];
        destination[04] = (char)lookup[unchecked((int)((value >> 44) & 0xF))];
        destination[05] = (char)lookup[unchecked((int)((value >> 40) & 0xF))];
        destination[06] = (char)lookup[unchecked((int)((value >> 36) & 0xF))];
        destination[07] = (char)lookup[unchecked((int)((value >> 32) & 0xF))];

        destination[08] = (char)lookup[unchecked((int)((value >> 28) & 0xF))];
        destination[09] = (char)lookup[unchecked((int)((value >> 24) & 0xF))];
        destination[10] = (char)lookup[unchecked((int)((value >> 20) & 0xF))];
        destination[11] = (char)lookup[unchecked((int)((value >> 16) & 0xF))];
        destination[12] = (char)lookup[unchecked((int)((value >> 12) & 0xF))];
        destination[13] = (char)lookup[unchecked((int)((value >> 08) & 0xF))];
        destination[14] = (char)lookup[unchecked((int)((value >> 04) & 0xF))];
        destination[15] = (char)lookup[unchecked((int)((value >> 00) & 0xF))];
    }
}
