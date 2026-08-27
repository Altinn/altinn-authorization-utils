# Altinn.Cli Console App

The `Altinn.Cli.Jwks` console application provides a convenient and secure way to generate Json Web Keys.

## Installation

Install using `dotnet tool install -g altinn-jwks`.

## Usage

After installation, the `altinn-jwks` tool is available to use in your favorite terminal. You can execute `altinn-jwks --help` to get started:

```
> altinn-jwks --help

  Description:
    Console app for creating Json Web Keys

  Usage:
    altinn-jwks [command] [options] [[--] <additional arguments>...]]

  Options:
    -?, -h, --help  Show help and usage information
    --version       Show version information

  Commands:
    create <name>  Create a new key and add it to a keyset
    export         Export key sets
    list           List all keys sets

  Additional Arguments:
    Arguments passed to the application that is being run.

  Sample usage:
    altinn-jwks create my-app
```

### Key store location

By default, the tool will create and use JWKs in the current directory. You can specify a different store location using the `--store` option,
which is available for all commands. The environment variable `ALTINN_JWK_STORE` can also be used to set a default store location.

A store location can point to a local directory or a remote Azure Key Vault. In order to use a Key Vault location,
you must have the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/?view=azure-cli-latest) installed and be
[logged in](https://learn.microsoft.com/en-us/cli/azure/authenticate-azure-cli-interactively?view=azure-cli-latest) to your Azure account.
The Key Vault location should be specified in the format `https://<key-vault-name>.vault.azure.net/`.

> [!NOTE]
> The structure of the JWK store is an undocumented implementation detail, and should not be relied uppon.
> It can be changed arbitrarily, without this being considered a breaking change. Use the CLI to get keys/key-sets in the different formats.

#### Example

The following example shows how to create a key in a local subdirectory called `keys`:

```
> altinn-jwks create my-app --store keys/
  Generating key my-app-TEST.AAAA
  Generating key my-app-PROD.AAAA

> altinn-jwks export key my-app --store keys/ | jq
  {
    "alg": "RS256",
    "e": "AQAB",
    ...
  }
```

The following example shows how to create a key in a remote Key Vault:

```
> altinn-jwks create my-app --store https://example.vault.azure.net/
  Generating key my-app-TEST.AAAA
  Generating key my-app-PROD.AAAA

> altinn-jwks export key my-app --store https://example.vault.azure.net/ | jq
  {
    "alg": "RS256",
    "e": "AQAB",
    ...
  }
```

### Listing key-sets

The `list` command will list all key-sets available in the current store:

```
> altinn-jwks list

  app1 (TEST, PROD)
  app2 (TEST, PROD)
```

### Creating keys

The `create` command lets you create new JWKs, using a range of available options.

```
> altinn-jwks create --help

  Description:
    Create a new key and add it to a keyset

  Usage:
    altinn-jwks create <name> [options] [[--] <additional arguments>...]]

  Arguments:
    <name>  Name of the integration to generate a new key for

  Options:
    -e, --env, --environment <None|Prod|Test>  Comma-separated list of Json Web Key Set environments to use [default:
                                              Test, Prod]
    -s, --size                                 Key size in bits []
    -a, --alg, --algorithm                     The algorithm to use for the key [default: RS256]
    <ES256|ES384|ES512|RS256|RS384|RS512>
    -u, --use <enc|sig>                        Use for the JWK [default: sig]
    --suffix                                   Optional suffix to append to the key ID [default: BKBQ]
    -?, -h, --help                             Show help and usage information
    -s, --store                                The JWKs store to use. Either a directory or an Azure Key Vault URI
                                              [default: .]
```

### Exporting keys

The `export key` command allows you to export a specific key in your required format.

```
> altinn-jwks export key --help

  Description:
    Export the current private or public key

  Usage:
    altinn-jwks export key <name> [options] [[--] <additional arguments>...]]

  Arguments:
    <name>  Name of the key to export

  Options:
    -e, --env, --environment <Prod|Test>  Json Web Key Set environment to use [default: Test]
    -r, --variant <Private|Public>        Decides whether to export the private or the public key [default: Private]
    -b, --base64                          Outputs the base64 version of the key [default: False]
    -?, -h, --help                        Show help and usage information
    -s, --store                           The JWKs store to use. Either a directory or an Azure Key Vault URI [default: .]
```

#### Example Maskinporten key

```
Create the key
> altinn-jwks create maskinportclientkey

Export public key. Default matches format for maskinporten
> altinn-jwks export key maskinportclientkey -r Public

Export private key as base64 (depends on format used in your app)
> altinn-jwks export key maskinportclientkey -b
```

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on [GitHub](https://github.com/your/repository).

## License

This library is licensed under the [MIT License](LICENSE).
