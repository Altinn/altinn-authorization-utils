# Altinn.Cli Console App

The `Altinn.JwkGenerator` console application provides a convenient way to generate Json Web Keys.

## Installation

Install using `dotnet tool install -g altinn-jwks`.

## Usage

Use your favorite terminal and execute the `altinn-jwks`.

Here's an example of the `altinn-jwks` console application's built in help page:

```
Description:
  Creates a new JWK

Usage:
  altinn-jwks create <name> [options]

Arguments:
  <name>  Name of the integration to generate JWKs for.

Options:
  -d, -t, --dev, --test                                         Generate TEST keys. Defaults to true unless --prod is specified.
  -p, --prod                                                    Generate PROD keys. Defaults to true unless --test is specified.
  -s, --size <size>                                             Key size in bits.
  -a, --alg, --algorithm <ES256|ES384|ES512|RS256|RS384|RS512>  The algorithm to use for the key. [default: RS256]
  -u, --use <enc|sig>                                           Use for the JWK. [default: sig]
  -o, --out <DIR>                                               Output directory for the generated JWKs. [default: $PATH]
  -?, -h, --help                                                Show help and usage information
```

### Create command

To create a JWK using the default values simply execute the app with the `create` command:

```pwsh
.\altinn-jwks create br
```

Output:

```
Generating key br-TEST.2024-06-21 for key-set br-TEST
Generating key br-PROD.2024-06-21 for key-set br-PROD
```

Any of the `options` can be used to override defaults when executing the`create` command:

```powershell
.\altinn-jwks create br --size 4096 --alg RS512 --use sig --out ./keys
```

Output:

```powershell
Generating key br-TEST.2024-06-21 for key-set br-TEST
Generating key br-PROD.2024-06-21 for key-set br-PROD
```

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on [GitHub](https://github.com/your/repository).

## License

This library is licensed under the [MIT License](LICENSE).
