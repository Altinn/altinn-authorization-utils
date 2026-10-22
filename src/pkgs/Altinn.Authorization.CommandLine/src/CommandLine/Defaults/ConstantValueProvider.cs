using System.Diagnostics.CodeAnalysis;
using System.Collections;
using Altinn.Authorization.CommandLine.Help;

namespace Altinn.Authorization.CommandLine;

internal sealed class ConstantValueProvider<T>(T value)
    : DefaultValueProvider<T>
{
    public override DefaultValueProvider<TTarget> Bind<TTarget>()
    {
        if (this is DefaultValueProvider<TTarget> typed)
        {
            return typed;
        }

        if (value is TTarget compatible)
        {
            return FromValue(compatible);
        }

        if (value is null && default(TTarget) is null)
        {
            return FromValue(default(TTarget)!);
        }

        throw new ArgumentException($"The default value of type '{value?.GetType() ?? typeof(T)}' cannot be assigned to '{typeof(TTarget)}'.");
    }

    public override bool TryProvideValue(DefaultValueProviderContext context, out T provided, [NotNullWhen(false)] out string? error)
    {
        provided = value;
        error = null;
        return true;
    }

    public override IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context)
    {
        if (value is null)
        {
            yield break;
        }

        if (value is IEnumerable values and not string)
        {
            var first = true;
            foreach (var item in values)
            {
                var text = item?.ToString();
                if (text is null)
                {
                    continue;
                }

                if (!first)
                {
                    yield return ",";
                }

                first = false;
                yield return new(text, context.DefaultArgumentColor);
            }
        }
        else if (value is bool boolean)
        {
            if (boolean)
            {
                yield return new("true", context.DefaultArgumentColor);
            }
        }
        else
        {
            yield return new(value.ToString() ?? string.Empty, context.DefaultArgumentColor);
        }
    }
}
