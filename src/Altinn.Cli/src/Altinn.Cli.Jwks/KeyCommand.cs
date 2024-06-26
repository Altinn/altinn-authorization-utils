using System.Buffers.Text;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Altinn.Authorization.JwkGenerator;

[ExcludeFromCodeCoverage]
internal class KeyCommand
{
    public static Command Command => CreateCommand();

    private static Command CreateCommand()
    {
        var nameArg = new Argument<string>("name", "Name of the integration to generate JWKs for.");

        var prodOption = new Option<bool>(
            aliases: ["--prod", "-p"],
            description: "Generate PROD keys. Defaults to true unless --test is specified.");

        var base64Option = new Option<bool>(
            aliases: ["--base64", "-b"],
            description: "Output the base64 version of the key.");

        var cmd = new Command("key", "Get a JWK")
        {
            nameArg,
            prodOption,
            base64Option,
        };

        cmd.SetHandler(ExecuteAsync, nameArg, prodOption, base64Option, Program.JwkDirOption);
        return cmd;
    }

    private static Task<int> ExecuteAsync(
        string name,
        bool prod,
        bool base64,
        DirectoryInfo jwkDirectory)
    {
        var command = new KeyCommand(name, prod, base64, jwkDirectory);
        return command.ExecuteAsync();
    }

    private readonly string _name;
    private readonly bool _prod;
    private readonly bool _base64;
    private readonly DirectoryInfo _jwkDirectory;

    public KeyCommand(
        string name,
        bool prod,
        bool base64,
        DirectoryInfo jwkDirectory)
    {
        _name = name;
        _prod = prod;
        _base64 = base64;
        _jwkDirectory = jwkDirectory;
    }

    private Task<int> ExecuteAsync()
    {
        var environment = _prod ? "PROD" : "TEST";
        var keySetId = string.Create(CultureInfo.InvariantCulture, $"{_name}-{environment}");

        var privateKeyPath = Path.Combine(_jwkDirectory.FullName, $"{keySetId}.key.json");
        var privateKeyBase64Path = Path.Combine(_jwkDirectory.FullName, $"{keySetId}.key.base64");

        if (_base64)
        {
            return OutFile(privateKeyBase64Path);
        }
        else
        {
            return OutFile(privateKeyPath);
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
