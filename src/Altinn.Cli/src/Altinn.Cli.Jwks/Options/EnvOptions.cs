using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Options;

[ExcludeFromCodeCoverage]
internal static class EnvOptions
{
    private static readonly string OptionName = "--environment";
    private static readonly ImmutableArray<string> OptionAliases = ["--env", "-e"];

    public static Option<JsonWebKeySetEnvironment> Single { get; }
        = new(
            name: OptionName,
            aliases: [.. OptionAliases])
        {
            Description = "Json Web Key Set environment to use",
            DefaultValueFactory = DefaultEnvironment,
            CustomParser = ParseEnvironment,
        };

    public static Option<JsonWebKeySetEnvironments> Multiple { get; }
        = new(
            name: OptionName,
            aliases: [.. OptionAliases])
        {
            Description = "Comma-separated list of Json Web Key Set environments to use",
            DefaultValueFactory = DefaultEnvironments,
            CustomParser = ParseEnvironments,
        };

    private static JsonWebKeySetEnvironment DefaultEnvironment(ArgumentResult result)
        => JsonWebKeySetEnvironment.Test;

    private static JsonWebKeySetEnvironments DefaultEnvironments(ArgumentResult result)
        => JsonWebKeySetEnvironments.Test | JsonWebKeySetEnvironments.Prod;

    private static JsonWebKeySetEnvironment ParseEnvironment(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            return DefaultEnvironment(result);
        }

        if (result.Tokens is [var token])
        {
            var value = token.Value.Trim().ToLowerInvariant();
            if (value.Contains(','))
            {
                result.AddError("Only one environment can be specified");
                return DefaultEnvironment(result);
            }

            switch (value)
            {
                case "test":
                case "dev":
                case "development":
                    return JsonWebKeySetEnvironment.Test;

                case "prod":
                case "production":
                    return JsonWebKeySetEnvironment.Prod;

                default:
                    result.AddError($"Unknown environment: {value}");
                    return DefaultEnvironment(result);
            }
        }
        else
        {
            result.AddError("Only one environment can be specified");
            return DefaultEnvironment(result);
        }
    }

    private static JsonWebKeySetEnvironments ParseEnvironments(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            return DefaultEnvironments(result);
        }

        JsonWebKeySetEnvironments envs = JsonWebKeySetEnvironments.None;
        foreach (var token in result.Tokens) 
        {
            var value = token.Value.Trim().ToLowerInvariant();
            if (value.Contains(','))
            {
                result.AddError("Multiple environments can be specified, but not in a single token");
                return DefaultEnvironments(result);
            }
            switch (value)
            {
                case "test":
                case "dev":
                case "development":
                    envs |= JsonWebKeySetEnvironments.Test;
                    break;

                case "prod":
                case "production":
                    envs |= JsonWebKeySetEnvironments.Prod;
                    break;

                default:
                    result.AddError($"Unknown environment: {value}");
                    break;
            }
        }

        return envs;
    }
}
