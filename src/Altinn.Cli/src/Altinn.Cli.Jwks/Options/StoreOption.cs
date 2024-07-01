﻿using Altinn.Cli.Jwks.Commands;
using Altinn.Cli.Jwks.Stores;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Options;

[ExcludeFromCodeCoverage]
internal class StoreOption
    : Option<JsonWebKeySetStore>
{
    private const string JWK_STORE_ENV_NAME = "ALTINN_JWK_STORE";

    public StoreOption()
        : base(
            aliases: ["--store", "-s"],
            description: "The JWKS store to use",
            parseArgument: ParseStore,
            isDefault: true)
    {
    }

    public void UpdateHelp(HelpContext ctx)
    {
        ctx.HelpBuilder.CustomizeSymbol(
            BaseCommand.StoreOption,
            defaultValue: (_) => $"${JWK_STORE_ENV_NAME} || $PATH");
    }

    private static JsonWebKeySetStore ParseStore(ArgumentResult result)
    {
        string arg;

        if (result.Tokens.Count == 0)
        {
            var fromEnv = Environment.GetEnvironmentVariable(JWK_STORE_ENV_NAME);
            if (string.IsNullOrEmpty(fromEnv))
            {
                fromEnv = Environment.CurrentDirectory;
            }
            
            arg = fromEnv;
        }
        else if (result.Tokens is [var token])
        {
            arg = token.Value;
        }
        else
        {
            result.ErrorMessage = "Only one store can be specified";
            return null!;
        }

        var uriOptions = new UriCreationOptions
        {
        };
        if (!Uri.TryCreate(arg, in uriOptions, out var uri))
        {
            // Assume it's a relative path, and try again
            var fullPath = Path.GetFullPath(arg);
            if (!Uri.TryCreate(fullPath, in uriOptions, out uri))
            {
                result.ErrorMessage = $"Invalid URI: {arg}";
                return null!;
            }
        }

        if (uri.IsFile)
        {
            var path = uri.LocalPath;
            return new DirectoryJsonWebKeySetStore(new DirectoryInfo(path));
        }

        if (string.Equals("https", uri.Scheme, StringComparison.OrdinalIgnoreCase) && uri.Host.EndsWith("vault.azure.net", StringComparison.OrdinalIgnoreCase))
        {
            var client = new SecretClient(uri, new AzureCliCredential());
            return new KeyVaultJsonWebKeySetStore(client);
        }

        result.ErrorMessage = $"Unsupported URI scheme: {uri.Scheme}";
        return null!;
    }
}
