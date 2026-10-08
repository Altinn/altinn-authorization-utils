using System.Text.RegularExpressions;
using Slugify;

namespace Altinn.Authorization.RepoCtl.Model.Utils;

/// <summary>
/// Provides utility methods for generating slugs from strings, including converting camel case to kebab case and handling non-ASCII characters.
/// </summary>
public static partial class Slug
{
    private static SlugHelperForNonAsciiLanguages _slugHelper = CreateSlugHelper();

    private static SlugHelperForNonAsciiLanguages CreateSlugHelper()
    {
        var builder = new SlugHelperForNonAsciiLanguages();
        builder.Config.StringReplacements["."] = "-";
        builder.Config.StringReplacements[":"] = "-";
        return builder;
    }

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])")]
    private static partial Regex CascalCaseRegex { get; }

    private static string CamelCaseToKebabCase(string input)
        => CascalCaseRegex.Replace(input, "-$1").Trim().ToLower();

    /// <summary>
    /// Generates a URL-friendly slug from the specified string.
    /// </summary>
    /// <param name="value">The string to generate a slug from.</param>
    /// <returns>A URL-friendly slug representation of the input string.</returns>
    public static string Slugify(string value)
        => _slugHelper.GenerateSlug(CamelCaseToKebabCase(value));
}
