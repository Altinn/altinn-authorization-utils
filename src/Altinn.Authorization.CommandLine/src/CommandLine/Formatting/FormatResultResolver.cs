using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.CommandLine.Results;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// A <see cref="ICommandResultHandlerResolver"/> that resolves <see cref="ICommandResultHandler"/>
/// for types that are formattable using one or more of the registered <see cref="IFormat"/>s.
/// </summary>
internal sealed class FormatResultResolver
    : ICommandResultHandlerResolver
{
    private readonly ImmutableArray<Format> _formats;
    private readonly OptionFactory _optionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatResultResolver"/> class.
    /// </summary>
    public FormatResultResolver(
        IEnumerable<IFormat> formats,
        OptionFactory optionFactory,
        IServiceProvider serviceProvider)
    {
        var builder = formats.TryGetNonEnumeratedCount(out var count)
            ? ImmutableArray.CreateBuilder<Format>(count)
            : ImmutableArray.CreateBuilder<Format>();

        foreach (var format in formats)
        {
            var type = typeof(Format<>).MakeGenericType(format.GetType());
            var instance = (Format)Activator.CreateInstance(type, [format, serviceProvider])!;

            builder.Add(instance);
        }

        _formats = builder.ToImmutable();
        _optionFactory = optionFactory;
    }

    /// <inheritdoc/>
    public bool TryResolve(Type type, [NotNullWhen(true)] out ICommandResultHandler? handler)
    {
        var builder = ImmutableArray.CreateBuilder<ResolvedFormatter>();
        foreach (var format in _formats)
        {
            if (format.TryResolve(type, out var formatter))
            {
                builder.Add(formatter);
            }
        }

        if (builder.Count == 0)
        {
            handler = null;
            return false;
        }

        handler = new CommandResultHandler(builder.ToImmutable(), _optionFactory);
        return true;
    }

    private abstract class Format(IFormat format)
    {
        public string Name { get; } = format.Name;

        public abstract bool TryResolve(Type type, [NotNullWhen(true)] out ResolvedFormatter? formatter);
    }

    private sealed class Format<TFormat>
        : Format
        where TFormat : IFormat
    {
        private readonly TFormat _format;
        private readonly FormatResolver<TFormat> _resolver;

        public Format(TFormat format, IServiceProvider serviceProvider)
            : base(format)
        {
            _format = format;
            _resolver = serviceProvider.GetRequiredService<FormatResolver<TFormat>>();
        }

        public override bool TryResolve(Type type, [NotNullWhen(true)] out ResolvedFormatter? formatter)
        {
            if (_resolver.TryResolve(type, out var f))
            {
                formatter = new ResolvedFormatter<TFormat>(_format, f);
                return true;
            }

            formatter = null;
            return false;
        }
    }

    private abstract class ResolvedFormatter
    {
        public abstract string Name { get; }

        public abstract Task HandleResult(object value, CommandInvocationContext context, CancellationToken cancellationToken = default);
    }

    private sealed class ResolvedFormatter<TFormat>
        : ResolvedFormatter
        where TFormat : IFormat
    {
        private readonly TFormat _format;
        private readonly IFormatter<TFormat> _formatter;

        public override string Name => _format.Name;

        public ResolvedFormatter(TFormat format, IFormatter<TFormat> formatter)
        {
            Guard.IsNotNull(format);
            Guard.IsNotNull(formatter);

            _format = format;
            _formatter = formatter;
        }

        public override async Task HandleResult(object value, CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            var writer = new FormatWriter(context.Console, context.ServiceProvider);
            await _formatter.Format(value, _format, writer, cancellationToken);
        }
    }

    private sealed class CommandResultHandler
        : ICommandResultHandler
        , IAdditionalOptionsMetadata
    {
        private readonly ImmutableArray<ResolvedFormatter> _formatters;
        private readonly Option<ResolvedFormatter> _formatOption;

        IEnumerable<Option> IAdditionalOptionsMetadata.AdditionalOptions
            => [_formatOption];

        public CommandResultHandler(ImmutableArray<ResolvedFormatter> formatters, OptionFactory optionFactory)
        {
            _formatters = formatters;
            _formatOption = CreateFormatOption(formatters, optionFactory);
        }

        public Task HandleResult(object? result, CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            if (result == null)
            {
                return Task.CompletedTask;
            }

            var formatter = context.ParseResult.GetRequiredValue(_formatOption);
            return formatter.HandleResult(result, context, cancellationToken);
        }

        private static Option<ResolvedFormatter> CreateFormatOption(ImmutableArray<ResolvedFormatter> formatters, OptionFactory optionFactory)
        {
            var option = optionFactory.Create<ResolvedFormatter>(
                name: "--format",
                aliases: ["-o"],
                description: "Specifies the output format.",
                isRequired: false,
                defaultValueBox: new(formatters[0]));

            option.ParserAndValueFactory = CreateParserAndValueFactory(formatters);
            option.HelpCustomization.Argument = HelpDisplayArgumentCustomization.Create(formatters.Select(f => f.Name));
            option.HelpCustomization.GetDefaultValue = () => [formatters[0].Name];

            return option;
        }

        private static Func<ArgumentResult, ResolvedFormatter> CreateParserAndValueFactory(ImmutableArray<ResolvedFormatter> formatters)
            => (result) =>
            {
                if (result.Tokens.Count == 0)
                {
                    return formatters[0];
                }

                if (result.Tokens.Count > 1)
                {
                    result.AddError($"The --format option can only be specified once.");
                    return formatters[0];
                }

                var token = result.Tokens[0].Value;
                var formatter = formatters.FirstOrDefault(f => string.Equals(token, f.Name, StringComparison.Ordinal));
                if (formatter == null)
                {
                    result.AddError($"The specified format '{token}' is not recognized.");
                    return formatters[0];
                }

                return formatter;
            };
    }
}
