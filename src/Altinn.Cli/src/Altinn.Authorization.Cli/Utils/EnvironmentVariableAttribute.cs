using Spectre.Console.Cli;

namespace Altinn.Authorization.Cli.Utils;

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
