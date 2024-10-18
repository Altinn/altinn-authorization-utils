using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Altinn.Urn;

/// <summary>
/// A raw, untyped RFC 8141 URN.
/// </summary>
public sealed class Urn
    //: IEquatable<Urn>
    //, IEqualityOperators<Urn, Urn, bool>
    //, IParsable<Urn>
    //, ISpanParsable<Urn>
    //, IUtf8SpanParsable<Urn>
    //, IFormattable
    //, ISpanFormattable
    //, IUtf8SpanFormattable
{
    /// <summary>
    /// Gets the maximum length of a URN.
    /// </summary>
    public const int MaxLength = ushort.MaxValue;

    private readonly string _value;
    private readonly ushort _nssStart;
    private readonly ushort _rComponentStart;
    private readonly ushort _qComponentStart;
    private readonly ushort _fComponentStart;

    private Urn(string value, ushort nssStart, ushort rComponentStart, ushort qComponentStart, ushort fComponentStart)
    {
        _value = value;
        _nssStart = nssStart;
        _rComponentStart = rComponentStart;
        _qComponentStart = qComponentStart;
        _fComponentStart = fComponentStart;
    }

    #region Parsing
    private static readonly CharGroup _delimiterCandidates = new("?#");

    private static ParseResult TryParse(
        ReadOnlySpan<char> value, 
        string? origin, 
        IFormatProvider? provider)
    {
        ushort nssStart, rComponentStart, qComponentStart, fComponentStart;
        ParseError err;
        Parser<char> parser = new(value);
        if (parser.TooShort)
        {
            return ParseError.TooShort;
        }

        if (parser.TooLong)
        {
            return ParseError.TooLong;
        }

        err = TryParseNamespaceIdentifier(ref parser, out nssStart, out var hasComponents);
        if (err is not ParseError.Ok)
        {
            return err;
        }

        if (hasComponents)
        {
            err = TryParseComponents(ref parser, out rComponentStart, out qComponentStart, out fComponentStart);
            if (err is not ParseError.Ok)
            {
                return err;
            }
        }
        else
        {
            rComponentStart = qComponentStart = fComponentStart = (ushort)value.Length;
        }

        origin ??= new(value);
        return new Urn(origin, nssStart, rComponentStart, qComponentStart, fComponentStart);
    }

    private static ParseError TryParseNamespaceIdentifier(
        ref Parser<char> parser,
        out ushort nssStart,
        out bool hasMoreComponents)
    {
        hasMoreComponents = false;
        nssStart = 0;
        if (!parser.TryEat("urn:"))
        {
            return ParseError.MissingUrnPrefix;
        }

        if (!parser.EatUntil(':', out var nid) || !NamespaceIdentifier.IsValid(nid))
        {
            return ParseError.InvalidNamespaceIdentifier;
        }

        parser.Eat(':');
        nssStart = parser.Index;
        hasMoreComponents = parser.EatUntilAny(_delimiterCandidates, out var nss);
        if (!NamespaceSpecificString.IsValid(nss))
        {
            return ParseError.InvalidNamespaceSpecificString;
        }

        return ParseError.Ok;
    }

    private static ParseError TryParseComponents(
        ref Parser<char> parser,
        out ushort rComponentStart,
        out ushort qComponentStart,
        out ushort fComponentStart)
    {
        bool hasMore = true;
        ParseError err;
        rComponentStart = 0;
        qComponentStart = 0;
        fComponentStart = 0;

        if (parser.TryEat("?+"))
        {
            rComponentStart = (ushort)(parser.Index - 2);

            err = TryParseRComponent(ref parser, out hasMore);
            if (err is not ParseError.Ok)
            {
                return err;
            }
        }

        if (hasMore && parser.TryEat("?="))
        {
            qComponentStart = (ushort)(parser.Index - 2);
            if (rComponentStart == 0)
            {
                rComponentStart = qComponentStart;
            }

            err = TryParseQComponent(ref parser, out hasMore);
            if (err is not ParseError.Ok)
            {
                return err;
            }
        }

        if (hasMore && parser.TryEat('#'))
        {
            fComponentStart = (ushort)(parser.Index - 1);
            if (rComponentStart == 0)
            {
                rComponentStart = fComponentStart;
            }
            if (qComponentStart == 0)
            {
                qComponentStart = fComponentStart;
            }

            // TODO: validate fragment
        }

        return ParseError.Ok;
    }

    private static ParseError TryParseRComponent(
        ref Parser<char> parser,
        out bool hasMore)
    {
        hasMore = false;
        var start = parser.Index;

        while (true)
        {
            var maybeHasMore = parser.EatUntilAny(_delimiterCandidates, out _);
            if (!maybeHasMore)
            {
                break;
            }

            if (parser.Matches('#'))
            {
                hasMore = true;
                break;
            }

            if (parser.Matches("?="))
            {
                hasMore = true;
                break;
            }

            parser.Eat('?');
        }

        var end = parser.Index;
        var slice = parser.Slice(start, (ushort)(end - start));

        if (!RQComponent.IsValid(slice))
        {
            return ParseError.InvalidRComponent;
        }

        return ParseError.Ok;
    }

    private static ParseError TryParseQComponent(
        ref Parser<char> parser,
        out bool hasMore)
    {
        var start = parser.Index;

        hasMore = parser.EatUntil('#', out _);

        var end = parser.Index;
        var slice = parser.Slice(start, (ushort)(end - start));

        if (!RQComponent.IsValid(slice))
        {
            return ParseError.InvalidQComponent;
        }

        return ParseError.Ok;
    }

    private enum ParseError
        : byte
    {
        Ok = 0,
        TooShort,
        TooLong,
        MissingUrnPrefix,
        InvalidNamespaceIdentifier,
        InvalidNamespaceSpecificString,
        InvalidRComponent,
        InvalidQComponent,
        InvalidFComponent,
    }

    private enum ParseState
    {
        AssignedName,
        RComponent,
        QComponent,
        FComponent,
    }

    private readonly struct ParseResult(ParseError error, Urn? value)
    {
        public ParseError Error { get; } = error;
        public Urn? Value { get; } = value;

        public bool TryGetValue([MaybeNullWhen(false)] out Urn value)
        {
            value = Value;
            return Error == ParseError.Ok;
        }

        public bool IsError
            => Error != ParseError.Ok;

        public static implicit operator ParseResult(Urn value)
            => new(ParseError.Ok, value);

        public static implicit operator ParseResult(ParseError error)
            => new(error, null);
    }

    private ref struct Parser<T>
        where T : struct, IEquatable<T>
    {
        private readonly ReadOnlySpan<T> _value;
        private ReadOnlySpan<T> _rest;

        public Parser(ReadOnlySpan<T> value)
        {
            _value = value;
            _rest = value;
        }

        public readonly ushort Index
            => (ushort)(_value.Length - _rest.Length);

        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rest[0];
        }

        public readonly bool TooShort
            => _value.Length < 4;

        public readonly bool TooLong
            => _value.Length > MaxLength;

        public readonly ReadOnlySpan<T> Slice(ushort start, ushort length)
            => _value.Slice(start, length);

        public void Eat(T value)
        {
            Debug.Assert(!_rest.IsEmpty);
            Debug.Assert(_rest[0].Equals(value));

            _rest = _rest[1..];
        }

        public bool Matches(T value)
            => !_rest.IsEmpty && _rest[0].Equals(value);

        public bool Matches(ReadOnlySpan<T> value)
            => _rest.StartsWith(value);

        public bool TryEat(T value)
        {
            if (!Matches(value))
            {
                return false;
            }

            _rest = _rest[1..];
            return true;
        }

        public bool TryEat(ReadOnlySpan<T> value)
        {
            if (!Matches(value))
            {
                return false;
            }

            _rest = _rest[value.Length..];
            return true;
        }

        /// <summary>
        /// Eats the input until the specified value is found. The value is not eaten and not included in the result.
        /// </summary>
        /// <remarks>
        /// If the value is not found, the remaining input is returned and eaten.
        /// </remarks>
        /// <param name="value">The value to eat until.</param>
        /// <param name="result">The values eaten.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> was found in the remaining input, otherwise <see langword="false"/>.</returns>
        public bool EatUntil(T value, out ReadOnlySpan<T> result)
        {
            var index = _rest.IndexOf(value);

            if (index == -1)
            {
                result = _rest;
                _rest = [];
                return false;
            }

            result = _rest[..index];
            _rest = _rest[index..];
            return true;
        }

        /// <summary>
        /// Eats the input until any of the specified values is found. The value is not eaten and not included in the result.
        /// </summary>
        /// <remarks>
        /// If the value is not found, the remaining input is returned and eaten.
        /// </remarks>
        /// <param name="values">The values to eat until.</param>
        /// <param name="result">The values eaten.</param>
        /// <returns><see langword="true"/> if any of <paramref name="values"/> was found in the remaining input, otherwise <see langword="false"/>.</returns>
        public bool EatUntilAny(SearchValues<T> values, out ReadOnlySpan<T> result)
        {
            var index = _rest.IndexOfAny(values);

            if (index == -1)
            {
                result = _rest;
                _rest = [];
                return false;
            }

            result = _rest[..index];
            _rest = _rest[index..];
            return true;
        }
    }
    #endregion

    #region Namespace Identifier (NID)
    private record struct NamespaceIdentifier
    {
        private static readonly CharGroup _valid = CharGroup.AlphaNum + CharGroup.Dash;

        public static bool IsValid(ReadOnlySpan<char> value)
        {
            if (value.Length < 2 || value.Length > 32)
            {
                return false;
            }

            if (value[0] == '-' || value[^1] == '-')
            {
                return false;
            }

            return !value.ContainsAnyExcept(_valid);
        }
    }
    #endregion


    #region Namespace Specific String (NSS)
    private record struct NamespaceSpecificString
    {
        private static readonly CharGroup _valid = CharGroup.PChar + CharGroup.Slash;

        public static bool IsValid(ReadOnlySpan<char> value)
        {
            if (value.Length == 0)
            {
                return false;
            }

            if (value[0] == '/')
            {
                return false;
            }

            if (value.ContainsAnyExcept(_valid))
            {
                return false;
            }

            return PercentageEncoding.Validate(value);
        }
    }
    #endregion

    #region R-Component and Q-Component
    private record struct RQComponent
    {
        private static readonly CharGroup _valid = CharGroup.PChar + CharGroup.Slash + CharGroup.QMark;

        public static bool IsValid(ReadOnlySpan<char> value)
        {
            // minimum 1 character after the '?+' or '?='
            if (value.Length < 3)
            {
                return false;
            }

            Debug.Assert(value[0] == '?' && (value[1] == '+' || value[1] == '='));
            if (value.ContainsAnyExcept(_valid))
            {
                return false;
            }

            return PercentageEncoding.Validate(value);
        }
    }
    #endregion

    #region Percentage Encoding
    private static class PercentageEncoding
    {
        public static bool Validate(ReadOnlySpan<char> value)
        {
            while (value.Length > 0)
            {
                var pctIndex = value.IndexOf('%');
                if (pctIndex == -1)
                {
                    return true;
                }

                if (pctIndex + 2 >= value.Length)
                {
                    return false;
                }

                if (value[(pctIndex + 1)..(pctIndex + 2)].ContainsAnyExcept(CharGroup.HexDig))
                {
                    return false;
                }

                value = value[(pctIndex + 3)..];
            }

            return true;
        }
    }
    #endregion

    #region Character Groups
    [DebuggerDisplay("{_chars}")]
    private readonly struct CharGroup
    {
        public static CharGroup Alpha = new("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
        public static CharGroup Digit = new("0123456789");
        public static CharGroup AlphaNum = Alpha + Digit;
        public static CharGroup Dash = new("-");
        public static CharGroup Slash = new("/");
        public static CharGroup QMark = new("?");
        public static CharGroup HexDig = Digit + "abcdefABCDEF";
        public static CharGroup Unreserved = Alpha + Digit + "-._~";
        public static CharGroup SubDelims = new CharGroup("!$&'()*+,;=");
        public static CharGroup PChar = Unreserved + SubDelims + ":@%";

        private readonly string _display;
        private readonly SearchValues<char> _charSearchValues;
        private readonly SearchValues<byte> _byteSearchValues;

        internal CharGroup(string chars)
        {
            AssertAllAscii(chars);

            var charSet = new SortedSet<char>(chars);
            var charBuffer = ArrayPool<char>.Shared.Rent(charSet.Count);
            try
            {
                var index = 0;
                foreach (var c in charSet)
                {
                    charBuffer[index++] = c;
                }

                chars = new(charBuffer.AsSpan(0, index));
                _display = chars;
                _charSearchValues = SearchValues.Create(chars);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(charBuffer);
            }

            var byteBuffer = ArrayPool<byte>.Shared.Rent(chars.Length);
            try
            {
                for (int i = 0; i < chars.Length; i++)
                {
                    byteBuffer[i] = (byte)chars[i];
                }

                _byteSearchValues = SearchValues.Create(byteBuffer.AsSpan(0, chars.Length));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(byteBuffer);
            }

            [Conditional("DEBUG")]
            static void AssertAllAscii(string value)
            {
                var chars = value.AsSpan();
                for (int i = 0; i < chars.Length; i++)
                {
                    Debug.Assert(chars[i] < 128);
                }
            }
        }

        public static CharGroup operator +(CharGroup left, CharGroup right)
            => new($"{left._display}{right._display}");

        public static CharGroup operator +(CharGroup left, string right)
            => new($"{left._display}{right}");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SearchValues<char>(CharGroup group)
            => group._charSearchValues;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SearchValues<byte>(CharGroup group)
            => group._byteSearchValues;
    }
    #endregion
}
