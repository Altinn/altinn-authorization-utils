using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class FileValueParser<T>
{
    public bool TryParse(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
    {
        value = default!;
        if (!context.TryGetSingleValue(out var text, out error))
        {
            return false;
        }

        try
        {
            object? parsed;
            if (typeof(T) == typeof(FileInfo))
            {
                parsed = new FileInfo(text);
            }
            else if (typeof(T) == typeof(DirectoryInfo))
            {
                parsed = new DirectoryInfo(text);
            }
            else if (typeof(T) == typeof(FileSystemInfo))
            {
                if (Directory.Exists(text) || Path.EndsInDirectorySeparator(text))
                {
                    parsed = new DirectoryInfo(text);
                }
                else
                {
                    parsed = new FileInfo(text);
                }
            }
            else
            {
                parsed = null;
            }

            if (parsed is T typed)
            {
                value = typed;
                return true;
            }
        }
        catch (ArgumentException)
        {
            error = $"Invalid path: {text}";
            return false;
        }

        error = $"No value parser is registered for '{typeof(T)}'.";
        return false;
    }
}
