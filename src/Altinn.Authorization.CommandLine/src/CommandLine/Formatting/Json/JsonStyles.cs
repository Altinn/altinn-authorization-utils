using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Formatting.Json;

internal static class JsonStyles
{
    public static readonly Style Braces = Color.Gray;
    public static readonly Style Brackets = Color.Gray;
    public static readonly Style Member = Color.Blue;
    public static readonly Style Colon = Color.Yellow;
    public static readonly Style Comma = Color.Gray;
    public static readonly Style String = Color.Red;
    public static readonly Style Number = Color.Green;
    public static readonly Style Boolean = Color.Magenta;
    public static readonly Style Null = Color.Gray;
    public static readonly Style Comment = Color.Gray;
}
