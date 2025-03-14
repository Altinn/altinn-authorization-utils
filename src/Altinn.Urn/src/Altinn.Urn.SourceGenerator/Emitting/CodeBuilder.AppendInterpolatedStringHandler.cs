using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Altinn.Urn.SourceGenerator.Emitting;

internal readonly ref partial struct CodeBuilder
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InterpolatedStringHandler]
    public ref struct AppendInterpolatedStringHandler
    {
        // Implementation note:
        // As this type is only intended to be targeted by the compiler, public APIs eschew argument validation logic
        // in a variety of places, e.g. allowing a null input when one isn't expected to produce a NullReferenceException rather
        // than an ArgumentNullException.

        /// <summary>The associated CodeStringBuilder to which to append.</summary>
        private readonly CodeBuilder _builder;

        /// <summary>Creates a handler used to append an interpolated string into a <see cref="CodeStringBuilder"/>.</summary>
        /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="builder">The associated CodeBuilder to which to append.</param>
        /// <remarks>This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise be for members intended to be used directly.</remarks>
        public AppendInterpolatedStringHandler(int literalLength, int formattedCount, CodeBuilder builder)
        {
            _builder = builder;
        }

        /// <summary>Writes the specified string to the handler.</summary>
        /// <param name="value">The string to write.</param>
        public void AppendLiteral(string value) => _builder.Append(value.AsSpan());

        #region AppendFormatted

        #region AppendFormatted T

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T value)
        {
            if (value is IFormattable formattable)
            {
                AppendLiteral(formattable.ToString(format: null, formatProvider: null));
            }
            else if (value is not null)
            {
                AppendLiteral(value.ToString());
            }
        }

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted<T>(T value, string? format)
        {
            if (value is IFormattable formattable)
            {
                // constrained call avoiding boxing for value types
                AppendLiteral(formattable.ToString(format, formatProvider: null));
            }
            else if (value is not null)
            {
                AppendLiteral(value.ToString());
            }
        }

        #endregion

        #region AppendFormatted ReadOnlySpan<char>

        /// <summary>Writes the specified character span to the handler.</summary>
        /// <param name="value">The span to write.</param>
        public void AppendFormatted(ReadOnlySpan<char> value) => _builder.Append(value);

        #endregion

        #region AppendFormatted string

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        public void AppendFormatted(string? value)
        {
            AppendFormatted<string?>(value);
        }

        #endregion

        #endregion
    }
}
