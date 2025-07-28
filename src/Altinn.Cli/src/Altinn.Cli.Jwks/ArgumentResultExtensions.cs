using System.CommandLine;
using System.CommandLine.Parsing;

namespace Altinn.Cli.Jwks;

internal static class ArgumentResultExtensions
{
    // TODO: Resolve the underlying issue (or usage pattern) that requires this extension method?
    /// <summary>
    /// <para><see cref="SymbolResult.GetResult(System.CommandLine.Argument)"/> has changed behavior from
    /// version 2.0.0-beta4.22272.1 to version 2.0.0-beta5.25306.1, which for whatever reason results in
    /// possible NullReferenceExceptions during the listing of options in an uninitialized command module.</para>
    /// <para>This extension method bypasses that issue in a crude fasion.</para>
    /// </summary>
    public static OptionResult? TryGetResult(this ArgumentResult argumentResult, Option? option)
    {
        if (option is null)
        {
            return null;
        }
        
        try
        {
            return argumentResult.GetResult(option);
        } catch
        {
            return null;
        }
    }
}
