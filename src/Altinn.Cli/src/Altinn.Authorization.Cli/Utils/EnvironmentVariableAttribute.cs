using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Cli.Utils;

/// <summary>
/// Provides default value for an option from an environment variable.
/// </summary>
[ExcludeFromCodeCoverage]
public class EnvironmentVariableAttribute
    : ParameterValueProviderAttribute
{
    private readonly string _name;

    public EnvironmentVariableAttribute(string name)
    {
        _name = name;
    }

    public override bool TryGetValue(CommandParameterContext context, out object? result)
    {
        result = null;

        if (context.Value is null)
        {
            var valStr = Environment.GetEnvironmentVariable(_name);

            if (valStr is null)
            {
                return false;
            }

            result = valStr;
            return true;
        }

        return false;
    }
}
