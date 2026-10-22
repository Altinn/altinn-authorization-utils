using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class DefaultValueParser
    : IValueParser
{
    public bool CanParse(Type type) => true;

    public bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        => Parser<T>.Parse(context, out value, out error);

    private delegate bool ParseValue<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error);

    private static class Parser<T>
    {
        public static readonly ParseValue<T> Parse = Create();

        private static ParseValue<T> Create()
        {
            var type = typeof(T);
            if (type == typeof(bool))
            {
                return (ParseValue<T>)(object)(ParseValue<bool>)ParseBoolean;
            }

            if (type == typeof(bool?))
            {
                return (ParseValue<T>)(object)(ParseValue<bool?>)ParseNullableBoolean;
            }

            object parser;
            if (type == typeof(StringValues))
            {
                parser = new StringValuesParser();
            }
            else if (Nullable.GetUnderlyingType(type) is { } underlying)
            {
                parser = Activator.CreateInstance(typeof(NullableValueParser<>).MakeGenericType(underlying))!;
            }
            else if (type.IsEnum)
            {
                parser = new EnumValueParser<T>();
            }
            else if (GetElementType(type) is { } element)
            {
                parser = Activator.CreateInstance(typeof(CollectionValueParser<,>).MakeGenericType(type, element))!;
            }
            else if (type == typeof(string))
            {
                parser = new StringValueParser();
            }
            else if (type.GetInterfaces().Any(static iface => iface.IsGenericType
                && iface.GetGenericTypeDefinition() == typeof(IParsable<>) && iface.GenericTypeArguments[0] == typeof(T)))
            {
                parser = Activator.CreateInstance(typeof(ParsableValueParser<>).MakeGenericType(type))!;
            }
            else if (type == typeof(FileInfo) || type == typeof(DirectoryInfo) || type == typeof(FileSystemInfo))
            {
                parser = new FileValueParser<T>();
            }
            else
            {
                return Unsupported;
            }

            return parser.GetType().GetMethod(nameof(TryParse))!.CreateDelegate<ParseValue<T>>(parser);
        }

        private static bool Unsupported(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        {
            value = default!;
            error = $"No value parser is registered for '{typeof(T)}'.";
            return false;
        }
    }

    private static readonly ParsableValueParser<bool> _booleanParser = new();
    private static bool ParseBoolean(ValueParsingContext context, out bool value, [NotNullWhen(false)] out string? error)
    {
        // A bare boolean option represents true; tokenization and arity belong to System.CommandLine.
        if (context.Values.Count == 0)
        {
            value = true;
            error = null;
            return true;
        }

        var success = _booleanParser.TryParse(context, out var parsed, out error);
        value = success ? parsed : default;
        return success;
    }

    private static bool ParseNullableBoolean(ValueParsingContext context, out bool? value, [NotNullWhen(false)] out string? error)
    {
        var success = ParseBoolean(context, out var parsed, out error);
        value = success ? parsed : null;
        return success;
    }

    private static Type? GetElementType(Type type)
    {
        if (type.IsSZArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();
            if (definition == typeof(List<>) || definition == typeof(IEnumerable<>)
                || definition == typeof(IList<>) || definition == typeof(ICollection<>)
                || definition == typeof(IReadOnlyList<>) || definition == typeof(IReadOnlyCollection<>))
            {
                return type.GenericTypeArguments[0];
            }
        }

        return null;
    }
}
