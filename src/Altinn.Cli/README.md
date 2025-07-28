TODO: Update output and commands with latest changes



# Altinn.Cli Console App
The `Altinn.JwkGenerator` console application provides a convenient way to generate Json Web Keys.

## Installation
Install using `dotnet tool install -g altinn-jwks`.

## Usage
After installation, the `altinn-jwks` tool is available to use in your favorite terminal. You can execute `altinn-jwks --help` to get started:

```
Description:
  Console app for creating Json Web Keys

Usage:
  altinn-jwks [command] [options]

Options:
  -s, --store <store>  The JWKS store to use [default: $ALTINN_JWK_STORE || $PATH]
  --version            Show version information
  -?, -h, --help       Show help and usage information

Commands:
  create <name>  List all keys sets
  export         Export key sets
  list           List all keys sets
```

### Create command
The `create` command lets you create new JWKs, using a range of available options described below:

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

#### Example
Executing the `create` command with default settings will produce two keys, intended for use in _test_ and _prod_ environments respectively.

```
> altinn-jwks create my-app-key
  Generating key my-app-key.TEST
  Generating key my-app-key.PROD

> ls -l
  my-app-key.PROD.key.json
  my-app-key.PROD.pub.json
  my-app-key.TEST.key.json
  my-app-key.TEST.pub.json
```

If you wish to modify the default behavior, you can override the relevant options as required. For instance, the following command outputs a single rs512 key, with a larger size, to a subfolder called _"keys"_:
```
> altinn-jwks create my-app-key --size 4096 --alg RS512 --prod --store ./keys
  Generating key my-app-key.PROD

> ls -l keys/
  my-app-key.PROD.key.json
  my-app-key.PROD.pub.json
```

### Export command


### List command
The `list` command simply lists all key sets found in the given store (current directory if omitted).

```
> altinn-jwks list         
  my-app-key (TEST, PROD)

> altinn-jwks list --store keys/                                             
  my-app-key (PROD)
```


## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request on [GitHub](https://github.com/your/repository).

## License

This library is licensed under the [MIT License](LICENSE).
