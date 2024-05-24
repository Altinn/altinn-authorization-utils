using CommunityToolkit.Diagnostics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

internal static class EnumExtensions 
{
    /// <summary>
    /// Returns a value indicating whether the specified value has any of the specified flags set.
    /// </summary>
    /// <typeparam name="T">The enum kind.</typeparam>
    /// <param name="value">The enum value to check if contains any flags.</param>
    /// <param name="flags">The flags to check.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> contains any of the flags set in <paramref name="flags"/>,
    /// otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasAnyOf<T>(this T value, T flags)
        where T : struct, Enum
    {
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        
        if (underlyingType == typeof(byte) || underlyingType == typeof(sbyte))
        {
            ref byte selfValue = ref Unsafe.As<T, byte>(ref value);
            ref byte flagsValue = ref Unsafe.As<T, byte>(ref flags);
            return (selfValue & flagsValue) != 0;
        }

        if (underlyingType == typeof(short) || underlyingType == typeof(ushort))
        {
            ref short selfValue = ref Unsafe.As<T, short>(ref value);
            ref short flagsValue = ref Unsafe.As<T, short>(ref flags);
            return (selfValue & flagsValue) != 0;
        }

        if (underlyingType == typeof(int) || underlyingType == typeof(uint))
        {
            ref int selfValue = ref Unsafe.As<T, int>(ref value);
            ref int flagsValue = ref Unsafe.As<T, int>(ref flags);
            return (selfValue & flagsValue) != 0;
        }

        if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
        {
            ref long selfValue = ref Unsafe.As<T, long>(ref value);
            ref long flagsValue = ref Unsafe.As<T, long>(ref flags);
            return (selfValue & flagsValue) != 0;
        }

        return ThrowHelper.ThrowArgumentException<bool>("Invalid enum type.");
    }
}
