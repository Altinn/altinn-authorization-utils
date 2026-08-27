using System.Reflection;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Factory;

internal static class ParameterResolverHelper
{
    public static void WriteErrors(IAnsiConsole console, IReadOnlyDictionary<ParameterInfo, IReadOnlyList<string>> errors)
    {
        var tree = new Tree("Parameter resolution failed");
        foreach (var (parameter, parameterErrors) in errors)
        {
            if (parameterErrors.Count == 0)
            {
                continue;
            }

            var parameterNode = tree.AddNode(Markup.FromInterpolated($"[magenta]{TypeNameHelper.GetTypeDisplayName(parameter.ParameterType, fullName: false)}[/] [cyan]{parameter.Name}[/]"));
            foreach (var error in parameterErrors)
            {
                parameterNode.AddNode(new Text(error, Color.Red));
            }
        }

        console.Write(tree);
    }
}
