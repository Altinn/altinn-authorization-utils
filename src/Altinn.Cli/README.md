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
    create <name>  Creates a new key with the provided name and options
    export <name>  Exports a given key
    list           Lists all known keys
```

### Key store location
By default, the tool will create and use JWKs in the current directory. You can specify a different store location using the `--store` option,
which is available for all commands. The environment variable `ALTINN_JWK_STORE` can also be used to set a default store location.

A store location can point to a local directory or a remote Azure Key Vault. In order to use a Key Vault location,
you must have the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/?view=azure-cli-latest) installed and be 
[logged in](https://learn.microsoft.com/en-us/cli/azure/authenticate-azure-cli-interactively?view=azure-cli-latest) to your Azure account.
The Key Vault location should be specified in the format `https://<key-vault-name>.vault.azure.net/`.

#### Example
The following example shows how to create a key in a local subdirectory called _keys_:
```
> altinn-jwks create my-app-key --store keys/
  Generating key my-app-key-TEST
  Generating key my-app-key-PROD

> ls -l keys/
  my-app-key-PROD.key.json
  my-app-key-PROD.pub.json
  my-app-key-TEST.key.json
  my-app-key-TEST.pub.json
```

The following example shows how to create a key in a remote Key Vault, and then export it again to verify the result:
```
> altinn-jwks create my-app-key --test --store https://<key-vault-name>.vault.azure.net/
  Generating key my-app-key-TEST

> altinn-jwks export my-app-key --store https://<key-vault-name>.vault.azure.net/
  {
    "alg": "RS256",
    "e": "AQAB",
    ...
  }
```

### Creating keys
The `create` command lets you create new JWKs, using a range of available options.

```
> altinn-jwks create --help

  Description:
    Creates a new key with the provided name and options
  
  Usage:
    altinn-jwks create <name> [options] [[--] <additional arguments>...]]
  
  Arguments:
    <name>  Name of the generated key
  
  Options:
    -d, -t, --dev, --test                                         Generate TEST keys. Defaults to true unless --prod is specified [default: True]
    -p, --prod                                                    Generate PROD keys. Defaults to true unless --test is specified [default: True]
    -s, --size                                                    Key size in bits [default: 2048]
    -a, --alg, --algorithm <ES256|ES384|ES512|RS256|RS384|RS512>  The algorithm to use for the key [default: RS256]
    -u, --use <enc|sig>                                           Use for the JWK [default: sig]
    -?, -h, --help                                                Show help and usage information
    -s, --store                                                   The JWKs store to use. Either a directory or an Azure Key Vault URI [default: .]
```

#### Example
Executing the `create` command with default settings will produce two keys, intended for use in _test_ and _prod_ environments respectively.

```
> altinn-jwks create my-app-key
  Generating key my-app-key-TEST
  Generating key my-app-key-PROD

> ls -l
  my-app-key-PROD.key.json
  my-app-key-PROD.pub.json
  my-app-key-TEST.key.json
  my-app-key-TEST.pub.json
```

If you wish to modify the default behavior, you can override the relevant options as required. 
For instance, the following command outputs a _prod_ key in RS512 format with a larger size:

```
> altinn-jwks create my-app-key --size 4096 --alg RS512 --prod
  Generating key my-app-key-PROD

> ls -l
  my-app-key-PROD.key.json
  my-app-key-PROD.pub.json
```

### Exporting keys
The `export` command allows you to export a specific key in your required format.

```
> altinn-jwks export --help

  Description:
    Exports a given key
  
  Usage:
    altinn-jwks export <name> [options] [[--] <additional arguments>...]]
  
  Arguments:
    <name>  Name of the key to export
  
  Options:
    --env, --environment <Prod|Test>   Decides whether to export the TEST or PROD key [default: Test]
    -role, --variant <Private|Public>  Decides whether to export the private or the public key [default: Private]
    -b, --base64                       Outputs the base64 version of the key [default: False]
    -?, -h, --help                     Show help and usage information
    -s, --store <value>                The JWKs store to use. Either a directory or an Azure Key Vault URI [default: .]
```

#### Example
To export a _prod_ public key in JSON format, for instance to upload to an external service, you can use the following command:

```
> altinn-jwks export my-app-key --env prod --variant public
  {
    "alg": "RS256",
    "e": "AQAB",
    ...
  }
```

To export the private key from the same pair, in base64 format, you can use the following command:

```
> altinn-jwks export my-app-key --env prod --variant private --base64
  ewogICJhbGciOiAiUlM1MTIiLAogICJkI...
```

### Listing keys
The `list` command lists all key sets found in the given store. The results are filtered by the naming conventions used by this tool,
which means that only keys created with the `create` command will be listed.

```
> altinn-jwks list --help

  Description:
    Lists all known keys
  
  Usage:
    altinn-jwks list [options] [[--] <additional arguments>...]]
  
  Options:
    -?, -h, --help       Show help and usage information
    -s, --store <value>  The JWKs store to use. Either a directory or an Azure Key Vault URI [default: .]
```

#### Example
To list all keys in the current store, you can simply run:

```
> altinn-jwks list
  my-app-key1 (TEST, PROD)
  my-app-key2 (TEST, PROD)
```

## Contributing
Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on [GitHub](https://github.com/your/repository).

## License
This library is licensed under the [MIT License](../../LICENSE).
