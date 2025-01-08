using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// A pre-start logger for Altinn services.
/// </summary>
public sealed class AltinnPreStartLogger
{
    private static readonly Action<string> _writeToStdout = Console.Out.WriteLine;

    /// <summary>
    /// Gets the configuration key for disabling the pre-start logger.
    /// </summary>
    public static string DisableConfigKey { get; } = "Altinn:Logging:PreStart:Disable";

    /// <summary>
    /// Creates a new instance of the <see cref="AltinnPreStartLogger"/> class.
    /// </summary>
    /// <typeparam name="T">The name of the logger.</typeparam>
    /// <param name="config">The <see cref="IConfiguration"/> to use for the logger.</param>
    /// <returns>A new <see cref="AltinnPreStartLogger"/>.</returns>
    public static AltinnPreStartLogger Create<T>(IConfiguration config)
        => Create(config, typeof(T).Name);

    /// <summary>
    /// Creates a new instance of the <see cref="AltinnPreStartLogger"/> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/> to use for the logger.</param>
    /// <param name="name">The name of the logger.</param>
    /// <returns>A new <see cref="AltinnPreStartLogger"/>.</returns>
    public static AltinnPreStartLogger Create(IConfiguration config, string name)
    {
        var disabled = config.GetValue(DisableConfigKey, defaultValue: false);

        return new(name, !disabled);
    }

    private readonly string _name;
    private readonly bool _enabled;

    private AltinnPreStartLogger(string name, bool enabled)
    {
        _name = name;
        _enabled = enabled;
    }

    /// <summary>
    /// Logs the specified message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="callerMemberName">See <see cref="CallerMemberNameAttribute"/>.</param>
    public void Log(string message, [CallerMemberName] string callerMemberName = "")
    {
        var handler = new LogHandler(message.Length, 0, this, callerMemberName);
        handler.AppendLiteral(message);
        handler.LogTo(_writeToStdout);
    }

    /// <summary>
    /// Logs the specified message.
    /// </summary>
    /// <param name="handler">The message.</param>
    public void Log([InterpolatedStringHandlerArgument("")] ref LogHandler handler)
    {
        handler.LogTo(_writeToStdout);
    }

    /// <summary>
    /// Provides a handler used by the language compiler to log efficiently.
    /// </summary>
    [InterpolatedStringHandler]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct LogHandler
    {
        StringBuilder? _builder;
        StringBuilder.AppendInterpolatedStringHandler? _formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandler"/> struct.
        /// </summary>
        /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="callerMemberName">The caller member name.</param>
        public LogHandler(int literalLength, int formattedCount, AltinnPreStartLogger logger, [CallerMemberName] string callerMemberName = "")
        {
            if (logger._enabled)
            {
                _builder = new StringBuilder(literalLength * 10);
                _builder.Append("// ").Append(logger._name).Append('.').Append(callerMemberName).Append(": ");
                _formatter = new(literalLength, formattedCount, _builder);
            }
        }

        /// <summary>
        /// Logs the message to the logger.
        /// </summary>
        /// <param name="log">The log write method.</param>
        internal void LogTo(Action<string> log)
        {
            if (_builder is { } builder)
            {
                log(builder.ToString());
            }
        }

        /// <summary>
        /// Writes the specified string to the handler.
        /// </summary>
        /// <param name="literal">The string to write.</param>
        public void AppendLiteral(string literal)
        {
            _formatter?.AppendLiteral(literal);
        }

        /// <summary>
        /// Writes the specified value to the handler.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="value">The value to write.</param>
        public void AppendFormatted<T>(T value)
        {
            _formatter?.AppendFormatted(value);
        }

        /// <summary>
        /// Writes the specified value to the handler.
        /// </summary>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        public void AppendFormatted<T>(T value, string? format)
        {
            _formatter?.AppendFormatted(value, format);
        }

        /// <summary>
        /// Writes the specified character span to the handler.
        /// </summary>
        /// <param name="value">The span to write.</param>
        public void AppendFormatted(ReadOnlySpan<char> value)
        {
            _formatter?.AppendFormatted(value);
        }
    }
}
