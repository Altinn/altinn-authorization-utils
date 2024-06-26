using Microsoft.IdentityModel.Tokens;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal class ExportMaskinportenCommand
{
    public static Command Command => CreateCommand();

    private static Command CreateCommand()
    {
        var nameArg = new Argument<string>("name", "Name of the integration to generate JWKs for.");

        var prodOption = new Option<bool>(
            aliases: ["--prod", "-p"],
            description: "Generate PROD keys. Defaults to true unless --test is specified.");

        var cmd = new Command("maskinporten", "Export a key set for maskinporten")
        {
            nameArg,
            prodOption,
        };

        cmd.SetHandler(ExecuteAsync, nameArg, prodOption, Program.JwkDirOption);
        return cmd;
    }

    private static Task<int> ExecuteAsync(
        string name,
        bool prod,
        DirectoryInfo jwkDirectory)
    {
        var command = new ExportMaskinportenCommand(name, prod, jwkDirectory);
        return command.ExecuteAsync();
    }

    private readonly string _name;
    private readonly bool _prod;
    private readonly DirectoryInfo _jwkDirectory;

    public ExportMaskinportenCommand(
        string name,
        bool prod,
        DirectoryInfo jwkDirectory)
    {
        _name = name;
        _prod = prod;
        _jwkDirectory = jwkDirectory;
    }

    private async Task<int> ExecuteAsync()
    {
        var environment = _prod ? "PROD" : "TEST";
        var keySetId = string.Create(CultureInfo.InvariantCulture, $"{_name}-{environment}");

        var publicKeySetPath = Path.Combine(_jwkDirectory.FullName, $"{keySetId}.pub.json");
        var keySet = await LoadKeySet(publicKeySetPath);

        if (keySet is null)
        {
            Console.Error.WriteLine($"Key set not found or failed to parse: {publicKeySetPath}");
            return 1;
        }

        await JsonSerializer.SerializeAsync(Console.OpenStandardOutput(), keySet.Keys, JsonOptions.Options);
        return 0;
    }

    private async Task<JsonWebKeySet?> LoadKeySet(string path)
    {
        try
        {
            await using var keyFs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
            return await JsonSerializer.DeserializeAsync<JsonWebKeySet>(keyFs, JsonOptions.Options);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    private async Task<int> OutFile(string path)
    {
        await using var keyFs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
        await using var stdout = Console.OpenStandardOutput();
        await keyFs.CopyToAsync(stdout);
        return 0;
    }
}
