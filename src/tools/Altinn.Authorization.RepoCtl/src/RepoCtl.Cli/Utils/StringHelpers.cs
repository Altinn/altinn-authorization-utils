using System.Buffers;

namespace Altinn.Authorization.RepoCtl.Utils;

internal static class StringHelpers
{
    private static readonly SearchValues<char> _lowercaseAlphanumericCharacters
        = SearchValues.Create("abcdefghijklmnopqrstuvwxyz0123456789");
    private static readonly SearchValues<char> _imageTagCharacters
        = SearchValues.Create("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.-");

    public static bool ContainsNonLowercaseAlphanumeric(ReadOnlySpan<char> value)
        => value.ContainsAnyExcept(_lowercaseAlphanumericCharacters);

    public static bool ContainsInvalidImageTagCharacters(ReadOnlySpan<char> value)
        => value.ContainsAnyExcept(_imageTagCharacters);
}
