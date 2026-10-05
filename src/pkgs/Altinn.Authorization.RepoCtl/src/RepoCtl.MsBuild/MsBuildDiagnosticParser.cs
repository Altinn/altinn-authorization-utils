// Copyright (c) .NET Foundation and contributors.
// Licensed under the MIT license. See LICENSE.
// Taken from https://github.com/dotnet/msbuild/blob/6df695adfc78f70ed122e27378e56f55afccfefe/src/Shared/CanonicalError.cs

namespace Altinn.Authorization.RepoCtl.MsBuild;

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Altinn.Authorization.RepoCtl.Model.Checks;

/// <summary>
/// Functions for dealing with the specially formatted errors returned by
/// build tools.
/// </summary>
/// <remarks>
/// Various tools produce and consume CanonicalErrors in various formats.
///
/// DEVENV Format When Clicking on Items in the Output Window
/// (taken from env\msenv\core\findutil.cpp ParseLocation function)
///
///      v:\dir\file.ext (loc) : msg
///      \\server\share\dir\file.ext(loc):msg
///      url
///
///      loc:
///      (line)
///      (line-line)
///      (line,col)
///      (line,col-col)
///      (line,col,len)
///      (line,col,line,col)
///
/// DevDiv Build Process
/// (taken from tools\devdiv2.def)
///
///      To echo warnings and errors to the build console, the
///      "description block" must be recognized by build. To do this,
///      add a $(ECHO_COMPILING_COMMAND) or $(ECHO_PROCESSING_COMMAND)
///      to the first line of the description block, e.g.
///
///          $(ECHO_COMPILING_CMD) Resgen_$&lt;
///
///      Errors must have the format:
///
///          &lt;text&gt; : error [num]: &lt;msg&gt;
///
///      Warnings must have the format:
///
///          &lt;text&gt; : warning [num]: &lt;msg&gt;
/// </remarks>
public static partial class MsBuildDiagnosticParser
{
    // Defines the main pattern for matching messages.
    private const string OriginCategoryCodeTextExpressionPattern =
        // Beginning of line and any amount of whitespace.
        @"^\s*"
        // Match a [optional project number prefix 'ddd>'], single letter + colon + remaining filename, or
        // string with no colon followed by a colon.
        + @"(((?<ORIGIN>(((\d+>)?[a-zA-Z]?:[^:]*)|([^:]*))):)"
        // Origin may also be empty. In this case there's no trailing colon.
        + "|())"
        // Match the empty string or a string without a colon that ends with a space
        + "(?<SUBCATEGORY>(()|([^:]*? )))"
        // Match 'error' or 'warning'.
        + @"(?<CATEGORY>(error|warning))"
        // Match anything starting with a space that's not a colon/space, followed by a colon.
        // Error code is optional in which case "error"/"warning" can be followed immediately by a colon.
        + @"( \s*(?<CODE>[^: ]*))?\s*:"
        // Whatever's left on this line, including colons.
        + "(?<TEXT>.*)$";

    private const string OriginCategoryCodeTextExpression2Pattern =
        @"^\s*(?<ORIGIN>(?<FILENAME>.*):(?<LOCATION>(?<LINE>[0-9]*):(?<COLUMN>[0-9]*))):(?<CATEGORY> error| warning):(?<TEXT>.*)";

    // Matches and extracts filename and location from an 'origin' element.
    private const string FilenameLocationFromOriginPattern =
        "^" // Beginning of line
        + @"(\d+>)?" // Optional ddd> project number prefix
        + "(?<FILENAME>.*)" // Match anything.
        + @"\(" // Find a parenthesis.
        + @"(?<LOCATION>[\,,0-9,-]*)" // Match any combination of numbers and ',' and '-'
        + @"\)\s*" // Find the closing paren then any amount of spaces.
        + "$"; // End-of-line

    // Matches location that is a simple number.
    private const string LineFromLocationPattern = // Example: line
        "^" // Beginning of line
        + "(?<LINE>[0-9]*)" // Match any number.
        + "$"; // End-of-line

    // Matches location that is a range of lines.
    private const string LineLineFromLocationPattern = // Example: line-line
        "^" // Beginning of line
        + "(?<LINE>[0-9]*)" // Match any number.
        + "-" // Dash
        + "(?<ENDLINE>[0-9]*)" // Match any number.
        + "$"; // End-of-line

    // Matches location that is a line and column
    private const string LineColFromLocationPattern = // Example: line,col
        "^" // Beginning of line
        + "(?<LINE>[0-9]*)" // Match any number.
        + "," // Comma
        + "(?<COLUMN>[0-9]*)" // Match any number.
        + "$"; // End-of-line

    // Matches location that is a line and column-range
    private const string LineColColFromLocationPattern = // Example: line,col-col
        "^" // Beginning of line
        + "(?<LINE>[0-9]*)" // Match any number.
        + "," // Comma
        + "(?<COLUMN>[0-9]*)" // Match any number.
        + "-" // Dash
        + "(?<ENDCOLUMN>[0-9]*)" // Match any number.
        + "$"; // End-of-line

    // Matches location that is a line, column, and length.
    private const string LineColLengthFromLocationPattern =
        "^(?<LINE>[0-9]*),(?<COLUMN>[0-9]*),(?<LENGTH>[0-9]*)$";

    // Matches location that is line,col,line,col
    private const string LineColLineColFromLocationPattern = // Example: line,col,line,col
        "^" // Beginning of line
        + "(?<LINE>[0-9]*)" // Match any number.
        + "," // Comma
        + "(?<COLUMN>[0-9]*)" // Match any number.
        + "," // Dash
        + "(?<ENDLINE>[0-9]*)" // Match any number.
        + "," // Dash
        + "(?<ENDCOLUMN>[0-9]*)" // Match any number.
        + "$"; // End-of-line

    private static readonly SearchValues<string> _warningOrError
        = SearchValues.Create(["warning", "error"], StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(OriginCategoryCodeTextExpressionPattern, RegexOptions.IgnoreCase)]
    private static partial Regex OriginCategoryCodeTextExpression { get; }

    [GeneratedRegex(OriginCategoryCodeTextExpression2Pattern, RegexOptions.IgnoreCase)]
    private static partial Regex OriginCategoryCodeTextExpression2 { get; }

    [GeneratedRegex(FilenameLocationFromOriginPattern, RegexOptions.IgnoreCase)]
    private static partial Regex FilenameLocationFromOrigin { get; }

    [GeneratedRegex(LineFromLocationPattern, RegexOptions.IgnoreCase)]
    private static partial Regex LineFromLocation { get; }

    [GeneratedRegex(LineLineFromLocationPattern, RegexOptions.IgnoreCase)]
    private static partial Regex LineLineFromLocation { get; }

    [GeneratedRegex(LineColFromLocationPattern, RegexOptions.IgnoreCase)]
    private static partial Regex LineColFromLocation { get; }

    [GeneratedRegex(LineColColFromLocationPattern, RegexOptions.IgnoreCase)]
    private static partial Regex LineColColFromLocation { get; }

    [GeneratedRegex(LineColLengthFromLocationPattern, RegexOptions.IgnoreCase)]
    private static partial Regex LineColLengthFromLocation { get; }

    [GeneratedRegex(LineColLineColFromLocationPattern, RegexOptions.IgnoreCase)]
    private static partial Regex LineColLineColFromLocation { get; }

    /// <summary>
    /// Represents the parts of a decomposed canonical message.
    /// </summary>
    internal ref struct Parts
    {
        /// <summary>
        /// Defines the error category\severity level.
        /// </summary>
        internal enum Category
        {
            Warning,
            Error,
        }

        /// <summary>
        /// Name of the file or tool (not localized)
        /// </summary>
        internal ReadOnlySpan<char> origin;

        /// <summary>
        /// The line number.
        /// </summary>
        internal ushort line;

        /// <summary>
        /// The column number.
        /// </summary>
        internal ushort column;

        /// <summary>
        /// The ending line number.
        /// </summary>
        internal ushort endLine;

        /// <summary>
        /// The ending column number.
        /// </summary>
        internal ushort endColumn;

        /// <summary>
        /// The category/severity level
        /// </summary>
        internal Category category;

        /// <summary>
        /// The sub category (localized)
        /// </summary>
        internal ReadOnlySpan<char> subcategory;

        /// <summary>
        /// The error code (not localized)
        /// </summary>
        internal ReadOnlySpan<char> code;

        /// <summary>
        /// The project path from the optional MSBuild message suffix.
        /// </summary>
        internal ReadOnlySpan<char> project;

        /// <summary>
        /// The unparsed project properties following '::' in the MSBuild message suffix.
        /// </summary>
        internal ReadOnlySpan<char> props;

        /// <summary>
        /// The error message text (localized)
        /// </summary>
        internal string? text;
    }

    /// <summary>
    /// A small custom ushort conversion method that treats invalid entries as missing (0). This is done to work around tools
    /// that don't fully conform to the canonical message format - we still want to salvage what we can from the message.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>'value' converted to ushort or 0 if it can't be parsed or is outside the ushort range</returns>
    private static ushort ConvertToUInt16WithDefault(ReadOnlySpan<char> value)
    {
        ushort result;
        bool success = ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);

        if (!success)
        {
            result = 0;
        }

        return result;
    }

    /// <summary>
    /// Decompose an error or warning message into constituent parts. If the message isn't in the canonical form, return null.
    /// </summary>
    /// <remarks>This method is thread-safe, because the Regex class is thread-safe (per MSDN).</remarks>
    /// <param name="pathRoot">The root directory used to resolve relative paths.</param>
    /// <param name="message">The message to attempt parsing.</param>
    /// <param name="diagnostic">The parsed diagnostic.</param>
    /// <returns>Decomposed canonical message, or null.</returns>
    public static bool TryParse(
        DirectoryInfo pathRoot,
        ReadOnlySpan<char> message,
        [NotNullWhen(true)] out Diagnostic? diagnostic)
    {
        Parts parsedMessage;

        // An unusually long string causes pathologically slow Regex back-tracking.
        // To avoid that, only scan the first 400 characters. That's enough for
        // the longest possible prefix: MAX_PATH, plus a huge subcategory string, and an error location.
        // After the regex is done, we can append the overflow.
        ReadOnlySpan<char> messageOverflow = default;
        if (message.Length > 400)
        {
            messageOverflow = message[400..];
            message = message[..400];
        }

        // If a tool has a large amount of output that isn't an error or warning (eg., "dir /s %hugetree%")
        // the regex below is slow. It's faster to pre-scan for "warning" and "error"
        // and bail out if neither are present.
        parsedMessage = default;
        if (message.IndexOfAny(_warningOrError) < 0)
        {
            diagnostic = null;
            return false;
        }

        // First, split the message into three parts--Origin, Category, Code, Text.
        // Example,
        //      Main.cs(17,20):Command line warning CS0168: The variable 'foo' is declared but never used
        //      -------------- ------------ ------- ------  ----------------------------------------------
        //      Origin         SubCategory  Cat.    Code    Text
        //
        // To accommodate absolute filenames in Origin, tolerate a colon in the second position
        // as long as its preceded by a letter.
        //
        // Localization Note:
        //  Even in foreign-language versions of tools, the category field needs to be in English.
        //  Also, if origin is a tool name, then that needs to be in English.
        //
        //  Here's an example from the Japanese version of CL.EXE:
        //   cl : ???? ??? warning D4024 : ?????????? 'AssemblyInfo.cs' ?????????????????? ???????????
        //
        //  Here's an example from the Japanese version of LINK.EXE:
        //   AssemblyInfo.cpp : fatal error LNK1106: ???????????? ??????????????: 0x6580 ??????????
        string messageString = new string(message);
        Match match = OriginCategoryCodeTextExpression.Match(messageString);

        ReadOnlySpan<char> category;
        if (!match.Success)
        {
            // try again with the Clang/GCC matcher
            // Example,
            //       err.cpp:6:3: error: use of undeclared identifier 'force_an_error'
            //       -----------  -----  ---------------------------------------------
            //       Origin       Cat.   Text
            match = OriginCategoryCodeTextExpression2.Match(messageString);
            if (!match.Success)
            {
                diagnostic = null;
                return false;
            }

            category = match.Groups["CATEGORY"].ValueSpan.Trim();
            if (category.Equals("error", StringComparison.OrdinalIgnoreCase))
            {
                parsedMessage.category = Parts.Category.Error;
            }
            else if (category.Equals("warning", StringComparison.OrdinalIgnoreCase))
            {
                parsedMessage.category = Parts.Category.Warning;
            }
            else
            {
                // Not an error\warning message.
                diagnostic = null;
                return false;
            }

            parsedMessage.line = ConvertToUInt16WithDefault(match.Groups["LINE"].ValueSpan.Trim());
            parsedMessage.column = ConvertToUInt16WithDefault(match.Groups["COLUMN"].ValueSpan.Trim());
            parsedMessage.text = TrimConcat(match.Groups["TEXT"].ValueSpan, messageOverflow);
            parsedMessage.origin = match.Groups["FILENAME"].ValueSpan.Trim();

            string[] explodedText = parsedMessage.text.Split(['\''], StringSplitOptions.RemoveEmptyEntries);
            if (explodedText.Length > 0)
            {
                parsedMessage.code = $"G{XxHash32(explodedText[0]):X8}";
            }
            else
            {
                parsedMessage.code = "G00000000";
            }

            diagnostic = CreateDiagnostic(pathRoot, in parsedMessage);
            return true;

            static uint XxHash32(string input)
            {
                var bytes = MemoryMarshal.AsBytes(input.AsSpan());
                return System.IO.Hashing.XxHash32.HashToUInt32(bytes);
            }
        }

        var origin = match.Groups["ORIGIN"].ValueSpan.Trim();
        category = match.Groups["CATEGORY"].ValueSpan.Trim();
        parsedMessage.code = match.Groups["CODE"].ValueSpan.Trim();
        parsedMessage.text = TrimConcat(match.Groups["TEXT"].ValueSpan, messageOverflow);
        parsedMessage.subcategory = match.Groups["SUBCATEGORY"].ValueSpan.Trim();

        // Next, see if category is something that is recognized.
        if (category.Equals("error", StringComparison.OrdinalIgnoreCase))
        {
            parsedMessage.category = Parts.Category.Error;
        }
        else if (category.Equals("warning", StringComparison.OrdinalIgnoreCase))
        {
            parsedMessage.category = Parts.Category.Warning;
        }
        else
        {
            // Not an error\warning message.
            diagnostic = null;
            return false;
        }

        // Only MSBuild-format messages carry the optional project suffix. Parse it after
        // recombining the text so that suffixes beyond the 400-character limit are included.
        ParseProjectSuffix(ref parsedMessage);

        // Origin is not a simple file, but it still could be of the form,
        //  foo.cpp(location)
        match = FilenameLocationFromOrigin.Match(new string(origin));

        if (match.Success)
        {
            // The origin is in the form,
            //  foo.cpp(location)
            // Assume the filename exists, but don't verify it. What else could it be?
            string location = new(match.Groups["LOCATION"].ValueSpan.Trim());
            parsedMessage.origin = match.Groups["FILENAME"].ValueSpan.Trim();

            // Now, take apart the location. It can be one of these:
            //      loc:
            //      (line)
            //      (line-line)
            //      (line,col)
            //      (line,col-col)
            //      (line,col,len)
            //      (line,col,line,col)
            if (location.Length > 0)
            {
                match = LineFromLocation.Match(location);
                if (match.Success)
                {
                    parsedMessage.line = ConvertToUInt16WithDefault(match.Groups["LINE"].ValueSpan.Trim());
                }
                else
                {
                    match = LineLineFromLocation.Match(location);
                    if (match.Success)
                    {
                        parsedMessage.line = ConvertToUInt16WithDefault(match.Groups["LINE"].ValueSpan.Trim());
                        parsedMessage.endLine = ConvertToUInt16WithDefault(match.Groups["ENDLINE"].ValueSpan.Trim());
                    }
                    else
                    {
                        match = LineColFromLocation.Match(location);
                        if (match.Success)
                        {
                            parsedMessage.line = ConvertToUInt16WithDefault(match.Groups["LINE"].ValueSpan.Trim());
                            parsedMessage.column = ConvertToUInt16WithDefault(match.Groups["COLUMN"].ValueSpan.Trim());
                        }
                        else
                        {
                            match = LineColColFromLocation.Match(location);
                            if (match.Success)
                            {
                                parsedMessage.line = ConvertToUInt16WithDefault(match.Groups["LINE"].ValueSpan.Trim());
                                parsedMessage.column = ConvertToUInt16WithDefault(match.Groups["COLUMN"].ValueSpan.Trim());
                                parsedMessage.endColumn = ConvertToUInt16WithDefault(match.Groups["ENDCOLUMN"].ValueSpan.Trim());
                            }
                            else
                            {
                                match = LineColLineColFromLocation.Match(location);
                                if (match.Success)
                                {
                                    parsedMessage.line = ConvertToUInt16WithDefault(match.Groups["LINE"].ValueSpan.Trim());
                                    parsedMessage.column = ConvertToUInt16WithDefault(match.Groups["COLUMN"].ValueSpan.Trim());
                                    parsedMessage.endLine = ConvertToUInt16WithDefault(match.Groups["ENDLINE"].ValueSpan.Trim());
                                    parsedMessage.endColumn = ConvertToUInt16WithDefault(match.Groups["ENDCOLUMN"].ValueSpan.Trim());
                                }
                                else
                                {
                                    match = LineColLengthFromLocation.Match(location);
                                    if (match.Success)
                                    {
                                        parsedMessage.line = ConvertToUInt16WithDefault(match.Groups["LINE"].ValueSpan.Trim());
                                        parsedMessage.column = ConvertToUInt16WithDefault(match.Groups["COLUMN"].ValueSpan.Trim());

                                        // The end column is exclusive. Preserve missing or out-of-range values as 0.
                                        if (parsedMessage.column > 0
                                            && ushort.TryParse(match.Groups["LENGTH"].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var length))
                                        {
                                            var endColumn = parsedMessage.column + length;
                                            parsedMessage.endColumn = endColumn <= ushort.MaxValue ? (ushort)endColumn : (ushort)0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // The origin does not fit the filename(location) pattern.
            parsedMessage.origin = origin;
        }

        diagnostic = CreateDiagnostic(pathRoot, in parsedMessage);
        return true;

        // convert parsed message to a Diagnostic object and convert all relative paths
        // to absolute paths using the path root.
        static Diagnostic CreateDiagnostic(DirectoryInfo pathRoot, in Parts parts)
        {
            DiagnosticOrigin? origin = null;
            if (!parts.origin.IsEmpty)
            {
                var name = parts.origin;
                // A separator identifies a path; names without separators are preserved.
                if (name.Contains('/') || name.Contains('\\'))
                {
                    name = ResolvePath(pathRoot, name);
                }

                // A column without a line cannot be represented. Column-only end
                // positions refer to the start line; otherwise leave the line unset.
                var start = new LinePosition(parts.line, parts.line == 0 ? (ushort)0 : parts.column);
                var endLine = parts.endLine != 0 ? parts.endLine : parts.endColumn != 0 ? parts.line : (ushort)0;
                var end = new LinePosition(endLine, endLine == 0 ? (ushort)0 : parts.endColumn);
                origin = new DiagnosticOrigin(new string(name), start, end);
            }

            var projectContext = parts.project.IsEmpty
                ? null
                : new DiagnosticProjectContext(ResolvePath(pathRoot, parts.project.ToString()), NullIfEmpty(parts.props));

            return new Diagnostic(
                parts.category == Parts.Category.Error ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning,
                parts.text ?? string.Empty,
                origin,
                NullIfEmpty(parts.subcategory),
                NullIfEmpty(parts.code),
                projectContext);

            static string? NullIfEmpty(ReadOnlySpan<char> value)
                => value.IsEmpty ? null : new(value);

            static string ResolvePath(DirectoryInfo root, ReadOnlySpan<char> path)
            {
                // Recognize rooted paths independently of the host operating system.
                if (path.StartsWith("/", StringComparison.Ordinal)
                    || path.StartsWith(@"\", StringComparison.Ordinal)
                    || (path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':'))
                {
                    return new string(path);
                }

                return Path.GetFullPath(new string(path).Replace('\\', Path.DirectorySeparatorChar), root.FullName);
            }
        }
    }

    private static void ParseProjectSuffix(ref Parts parsedMessage)
    {
        var text = parsedMessage.text.AsSpan().TrimEnd();
        if (!text.EndsWith("]", StringComparison.Ordinal))
        {
            return;
        }

        var start = text.LastIndexOf(" [", StringComparison.Ordinal);
        if (start < 0)
        {
            return;
        }

        var suffix = text[(start + 2)..^1];
        var separator = suffix.IndexOf("::", StringComparison.Ordinal);
        var project = separator < 0 ? suffix : suffix[..separator];
        if (project.IsEmpty || suffix.Contains('[') || suffix.Contains(']'))
        {
            return;
        }

        parsedMessage.project = project;
        parsedMessage.props = separator < 0 ? default : suffix[(separator + 2)..];
        parsedMessage.text = text[..start].TrimEnd().ToString();
    }

    private static string TrimConcat(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        left = left.TrimStart();
        right = right.TrimEnd();

        return string.Create(length: left.Length + right.Length, state: new TrimConcatState(left, right), action: (span, state) =>
        {
            state.Left.CopyTo(span);
            state.Right.CopyTo(span[state.Left.Length..]);
        });
    }

    private readonly ref struct TrimConcatState(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
        private readonly ReadOnlySpan<char> _left = left;
        private readonly ReadOnlySpan<char> _right = right;

        public readonly ReadOnlySpan<char> Left => _left;
        public readonly ReadOnlySpan<char> Right => _right;
    }
}
